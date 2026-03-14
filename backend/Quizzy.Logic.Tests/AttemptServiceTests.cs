using Quizzy.Data.Models;
using Quizzy.Logic.DTOs;
using Quizzy.Logic.Services;

namespace Quizzy.Logic.Tests;

public class AttemptServiceTests : IDisposable
{
    private readonly TestDb _testDb = new();
    private readonly AttemptService _sut;

    public AttemptServiceTests()
    {
        _sut = new AttemptService(_testDb.Context, new SimilarityService());
    }

    public void Dispose() => _testDb.Dispose();

    private async Task<(User User, Quiz Quiz, Question McQuestion, AnswerOption CorrectOption, Question OtQuestion)> SeedData()
    {
        var user = new User { Username = "player", Email = "player@test.com", PasswordHash = "hash" };
        var quiz = new Quiz
        {
            Title = "Test Quiz",
            Description = "Desc",
            TimeLimitPerQuestion = 30,
            IsActive = true
        };
        _testDb.Context.Users.Add(user);
        _testDb.Context.Quizzes.Add(quiz);
        await _testDb.Context.SaveChangesAsync();

        var mcQuestion = new Question
        {
            QuizId = quiz.Id,
            Text = "MC Question",
            Type = QuestionType.MultipleChoice,
            OrderIndex = 0,
            Points = 10,
        };
        var otQuestion = new Question
        {
            QuizId = quiz.Id,
            Text = "OT Question",
            Type = QuestionType.OpenText,
            OrderIndex = 1,
            Points = 10,
        };
        _testDb.Context.Questions.AddRange(mcQuestion, otQuestion);
        await _testDb.Context.SaveChangesAsync();

        var correctOption = new AnswerOption { QuestionId = mcQuestion.Id, Text = "Correct", IsCorrect = true };
        var wrongOption = new AnswerOption { QuestionId = mcQuestion.Id, Text = "Wrong", IsCorrect = false };
        _testDb.Context.AnswerOptions.AddRange(correctOption, wrongOption);

        var openAnswer = new OpenTextAnswer { QuestionId = otQuestion.Id, Text = "photosynthesis", SimilarityThreshold = 0.7 };
        _testDb.Context.OpenTextAnswers.Add(openAnswer);
        await _testDb.Context.SaveChangesAsync();

        return (user, quiz, mcQuestion, correctOption, otQuestion);
    }

    [Fact]
    public async Task StartAttemptCreatesNewAttempt()
    {
        var (user, quiz, _, _, _) = await SeedData();

        var result = await _sut.StartAttempt(user.Id, new StartAttemptRequest(quiz.Id));

        Assert.Equal(quiz.Id, result.QuizId);
        Assert.Equal("Test Quiz", result.QuizTitle);
        Assert.Equal(0, result.TotalScore);
        Assert.Null(result.CompletedAt);
    }

    [Fact]
    public async Task StartAttemptWhenQuizNotFoundThenThrows()
    {
        var user = new User { Username = "u", Email = "u@t.com", PasswordHash = "h" };
        _testDb.Context.Users.Add(user);
        await _testDb.Context.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.StartAttempt(user.Id, new StartAttemptRequest(999)));

        Assert.Equal("Quiz not found.", ex.Message);
    }

    [Fact]
    public async Task StartAttemptWhenQuizNotActiveThenThrows()
    {
        var user = new User { Username = "u", Email = "u@t.com", PasswordHash = "h" };
        var quiz = new Quiz { Title = "Q", Description = "D", IsActive = false };
        _testDb.Context.Users.Add(user);
        _testDb.Context.Quizzes.Add(quiz);
        await _testDb.Context.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.StartAttempt(user.Id, new StartAttemptRequest(quiz.Id)));

        Assert.Equal("Quiz is not active.", ex.Message);
    }

    [Fact]
    public async Task SubmitAnswerForMultipleChoiceCorrectThenAwardsPoints()
    {
        var (user, quiz, mcQuestion, correctOption, _) = await SeedData();
        var attempt = await _sut.StartAttempt(user.Id, new StartAttemptRequest(quiz.Id));

        var result = await _sut.SubmitAnswer(user.Id, attempt.Id,
            new SubmitAnswerRequest(mcQuestion.Id, correctOption.Id, null, 5));

        Assert.True(result.IsCorrect);
        Assert.Equal(10, result.PointsEarned);
    }

    [Fact]
    public async Task SubmitAnswerForMultipleChoiceWrongThenAwardsZeroPoints()
    {
        var (user, quiz, mcQuestion, _, _) = await SeedData();
        var attempt = await _sut.StartAttempt(user.Id, new StartAttemptRequest(quiz.Id));
        var wrongOption = _testDb.Context.AnswerOptions
            .First(a => a.QuestionId == mcQuestion.Id && !a.IsCorrect);

        var result = await _sut.SubmitAnswer(user.Id, attempt.Id,
            new SubmitAnswerRequest(mcQuestion.Id, wrongOption.Id, null, 5));

        Assert.False(result.IsCorrect);
        Assert.Equal(0, result.PointsEarned);
    }

    [Fact]
    public async Task SubmitAnswerForOpenTextWithHighSimilarityThenAwardsPoints()
    {
        var (user, quiz, _, _, otQuestion) = await SeedData();
        var attempt = await _sut.StartAttempt(user.Id, new StartAttemptRequest(quiz.Id));

        var result = await _sut.SubmitAnswer(user.Id, attempt.Id,
            new SubmitAnswerRequest(otQuestion.Id, null, "photosynthesis", 5));

        Assert.True(result.PointsEarned > 0);
        Assert.NotNull(result.SimilarityScore);
    }

    [Fact]
    public async Task SubmitAnswerForOpenTextWithLowSimilarityThenAwardsZeroPoints()
    {
        var (user, quiz, _, _, otQuestion) = await SeedData();
        var attempt = await _sut.StartAttempt(user.Id, new StartAttemptRequest(quiz.Id));

        var result = await _sut.SubmitAnswer(user.Id, attempt.Id,
            new SubmitAnswerRequest(otQuestion.Id, null, "completely unrelated answer xyz", 5));

        Assert.Equal(0, result.PointsEarned);
    }

    [Fact]
    public async Task SubmitAnswerWhenAttemptNotFoundThenThrows()
    {
        var (user, _, mcQuestion, correctOption, _) = await SeedData();

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.SubmitAnswer(user.Id, 999,
                new SubmitAnswerRequest(mcQuestion.Id, correctOption.Id, null, 5)));

        Assert.Equal("Attempt not found.", ex.Message);
    }

    [Fact]
    public async Task SubmitAnswerWhenAttemptCompletedThenThrows()
    {
        var (user, quiz, mcQuestion, correctOption, _) = await SeedData();
        var attempt = await _sut.StartAttempt(user.Id, new StartAttemptRequest(quiz.Id));
        await _sut.CompleteAttempt(user.Id, attempt.Id);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.SubmitAnswer(user.Id, attempt.Id,
                new SubmitAnswerRequest(mcQuestion.Id, correctOption.Id, null, 5)));

        Assert.Equal("Attempt already completed.", ex.Message);
    }

    [Fact]
    public async Task SubmitAnswerWhenQuestionAlreadyAnsweredThenThrows()
    {
        var (user, quiz, mcQuestion, correctOption, _) = await SeedData();
        var attempt = await _sut.StartAttempt(user.Id, new StartAttemptRequest(quiz.Id));
        await _sut.SubmitAnswer(user.Id, attempt.Id,
            new SubmitAnswerRequest(mcQuestion.Id, correctOption.Id, null, 5));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.SubmitAnswer(user.Id, attempt.Id,
                new SubmitAnswerRequest(mcQuestion.Id, correctOption.Id, null, 5)));

        Assert.Equal("Question already answered.", ex.Message);
    }

    [Fact]
    public async Task CompleteAttemptCalculatesScoresAndTimeBonus()
    {
        var (user, quiz, mcQuestion, correctOption, _) = await SeedData();
        var attempt = await _sut.StartAttempt(user.Id, new StartAttemptRequest(quiz.Id));
        // Answer correctly with fast time (10s out of 30s time limit)
        await _sut.SubmitAnswer(user.Id, attempt.Id,
            new SubmitAnswerRequest(mcQuestion.Id, correctOption.Id, null, 10));

        var result = await _sut.CompleteAttempt(user.Id, attempt.Id);

        Assert.NotNull(result.CompletedAt);
        Assert.Equal(10, result.BaseScore);
        // Time bonus: floor((30 - 10) / 30 * 5) = floor(3.33) = 3
        Assert.Equal(3, result.TimeBonus);
        Assert.Equal(13, result.TotalScore);
    }

    [Fact]
    public async Task CompleteAttemptWhenAlreadyCompletedThenThrows()
    {
        var (user, quiz, _, _, _) = await SeedData();
        var attempt = await _sut.StartAttempt(user.Id, new StartAttemptRequest(quiz.Id));
        await _sut.CompleteAttempt(user.Id, attempt.Id);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.CompleteAttempt(user.Id, attempt.Id));

        Assert.Equal("Attempt already completed.", ex.Message);
    }

    [Fact]
    public async Task GetHistoryReturnsUserAttempts()
    {
        var (user, quiz, _, _, _) = await SeedData();
        await _sut.StartAttempt(user.Id, new StartAttemptRequest(quiz.Id));
        await _sut.StartAttempt(user.Id, new StartAttemptRequest(quiz.Id));

        var result = await _sut.GetHistory(user.Id);

        Assert.Equal(2, result.Count);
    }
}
