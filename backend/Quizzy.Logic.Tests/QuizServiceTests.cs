using Quizzy.Data.Models;
using Quizzy.Logic.DTOs;
using Quizzy.Logic.Services;

namespace Quizzy.Logic.Tests;

public class QuizServiceTests : IDisposable
{
    private readonly TestDb _testDb = new();
    private readonly QuizService _sut;

    public QuizServiceTests()
    {
        _sut = new QuizService(_testDb.Context);
    }

    public void Dispose() => _testDb.Dispose();

    [Fact]
    public async Task GetAllReturnsAllQuizzes()
    {
        _testDb.Context.Quizzes.AddRange(
            new Quiz { Title = "Quiz 1", Description = "Desc 1" },
            new Quiz { Title = "Quiz 2", Description = "Desc 2", IsActive = false }
        );
        await _testDb.Context.SaveChangesAsync();

        var result = await _sut.GetAll();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetActiveReturnsOnlyActiveQuizzes()
    {
        _testDb.Context.Quizzes.AddRange(
            new Quiz { Title = "Active", Description = "Desc", IsActive = true },
            new Quiz { Title = "Inactive", Description = "Desc", IsActive = false }
        );
        await _testDb.Context.SaveChangesAsync();

        var result = await _sut.GetActive();

        Assert.Single(result);
        Assert.Equal("Active", result[0].Title);
    }

    [Fact]
    public async Task GetByIdWhenQuizExistsThenReturnsQuizWithQuestions()
    {
        var quiz = new Quiz
        {
            Title = "Test Quiz",
            Description = "Description",
            Questions =
            [
                new Question
                {
                    Text = "Q1",
                    Type = QuestionType.MultipleChoice,
                    OrderIndex = 0,
                    Points = 10,
                    AnswerOptions = [new AnswerOption { Text = "A", IsCorrect = true }]
                }
            ]
        };
        _testDb.Context.Quizzes.Add(quiz);
        await _testDb.Context.SaveChangesAsync();

        var result = await _sut.GetById(quiz.Id, includeCorrectAnswers: true);

        Assert.Equal("Test Quiz", result.Title);
        Assert.Single(result.Questions);
        Assert.True(result.Questions[0].AnswerOptions[0].IsCorrect);
    }

    [Fact]
    public async Task GetByIdWhenExcludeCorrectAnswersThenHidesInfo()
    {
        var quiz = new Quiz
        {
            Title = "Test Quiz",
            Description = "Description",
            Questions =
            [
                new Question
                {
                    Text = "Q1",
                    Type = QuestionType.OpenText,
                    OrderIndex = 0,
                    Points = 10,
                    AnswerOptions = [new AnswerOption { Text = "A", IsCorrect = true }],
                    OpenTextAnswers = [new OpenTextAnswer { Text = "answer", SimilarityThreshold = 0.7 }]
                }
            ]
        };
        _testDb.Context.Quizzes.Add(quiz);
        await _testDb.Context.SaveChangesAsync();

        var result = await _sut.GetById(quiz.Id, includeCorrectAnswers: false);

        Assert.False(result.Questions[0].AnswerOptions[0].IsCorrect);
        Assert.Empty(result.Questions[0].OpenTextAnswers);
    }

    [Fact]
    public async Task GetByIdWhenQuizNotFoundThenThrows()
    {
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.GetById(999, includeCorrectAnswers: false));

        Assert.Equal("Quiz not found.", ex.Message);
    }

    [Fact]
    public async Task CreateThenReturnsCreatedQuiz()
    {
        var request = new CreateQuizRequest("New Quiz", "A quiz", Difficulty.Medium, 30);

        var result = await _sut.Create(request);

        Assert.Equal("New Quiz", result.Title);
        Assert.Equal(Difficulty.Medium, result.Difficulty);
        Assert.Equal(30, result.TimeLimitPerQuestion);
        Assert.Equal(0, result.QuestionCount);
    }

    [Fact]
    public async Task UpdateWhenQuizExistsThenUpdatesFields()
    {
        var quiz = new Quiz { Title = "Old Title", Description = "Old" };
        _testDb.Context.Quizzes.Add(quiz);
        await _testDb.Context.SaveChangesAsync();

        var request = new UpdateQuizRequest("New Title", "New Desc", Difficulty.Hard, 60, false);
        var result = await _sut.Update(quiz.Id, request);

        Assert.Equal("New Title", result.Title);
        Assert.Equal("New Desc", result.Description);
        Assert.Equal(Difficulty.Hard, result.Difficulty);
        Assert.Equal(60, result.TimeLimitPerQuestion);
        Assert.False(result.IsActive);
    }

    [Fact]
    public async Task UpdateWhenQuizNotFoundThenThrows()
    {
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.Update(999, new UpdateQuizRequest("T", "D", Difficulty.Easy, 30, true)));

        Assert.Equal("Quiz not found.", ex.Message);
    }

    [Fact]
    public async Task DeleteWhenQuizExistsThenRemovesQuiz()
    {
        var quiz = new Quiz { Title = "To Delete", Description = "Desc" };
        _testDb.Context.Quizzes.Add(quiz);
        await _testDb.Context.SaveChangesAsync();

        await _sut.Delete(quiz.Id);

        Assert.Empty(_testDb.Context.Quizzes);
    }

    [Fact]
    public async Task DeleteWhenQuizNotFoundThenThrows()
    {
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.Delete(999));

        Assert.Equal("Quiz not found.", ex.Message);
    }

    [Fact]
    public async Task GetQuestionsForPlayReturnsQuestionsInOrder()
    {
        var quiz = new Quiz
        {
            Title = "Quiz",
            Description = "Desc",
            TimeLimitPerQuestion = 30,
            Questions =
            [
                new Question { Text = "Q2", OrderIndex = 1, Points = 10, Type = QuestionType.MultipleChoice },
                new Question { Text = "Q1", OrderIndex = 0, Points = 5, Type = QuestionType.MultipleChoice }
            ]
        };
        _testDb.Context.Quizzes.Add(quiz);
        await _testDb.Context.SaveChangesAsync();

        var result = await _sut.GetQuestionsForPlay(quiz.Id);

        Assert.Equal(2, result.Count);
        Assert.Equal("Q1", result[0].Text);
        Assert.Equal("Q2", result[1].Text);
        Assert.Equal(30, result[0].TimeLimit);
    }

    [Fact]
    public async Task GetQuestionsForPlayWhenQuizNotFoundThenThrows()
    {
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.GetQuestionsForPlay(999));

        Assert.Equal("Quiz not found.", ex.Message);
    }
}
