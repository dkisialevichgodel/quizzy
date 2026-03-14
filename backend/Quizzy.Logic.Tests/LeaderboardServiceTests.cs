using Quizzy.Data.Models;
using Quizzy.Logic.Services;

namespace Quizzy.Logic.Tests;

public class LeaderboardServiceTests : IDisposable
{
    private readonly TestDb _testDb = new();
    private readonly LeaderboardService _sut;

    public LeaderboardServiceTests()
    {
        _sut = new LeaderboardService(_testDb.Context);
    }

    public void Dispose() => _testDb.Dispose();

    [Fact]
    public async Task GetLeaderboardWhenNoAttemptsThenReturnsEmpty()
    {
        var result = await _sut.GetLeaderboard(Difficulty.Easy);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetLeaderboardReturnsEntriesOrderedByBestScore()
    {
        var user1 = new User { Username = "player1", Email = "p1@test.com", PasswordHash = "h" };
        var user2 = new User { Username = "player2", Email = "p2@test.com", PasswordHash = "h" };
        var quiz = new Quiz { Title = "Q", Description = "D", Difficulty = Difficulty.Easy, IsActive = true };
        _testDb.Context.Users.AddRange(user1, user2);
        _testDb.Context.Quizzes.Add(quiz);
        await _testDb.Context.SaveChangesAsync();

        _testDb.Context.Questions.Add(
            new Question { QuizId = quiz.Id, Text = "Q1", Points = 10, Type = QuestionType.MultipleChoice });
        await _testDb.Context.SaveChangesAsync();

        _testDb.Context.QuizAttempts.AddRange(
            new QuizAttempt { UserId = user1.Id, QuizId = quiz.Id, IsCompleted = true, TotalScore = 100, CompletedAt = DateTime.UtcNow },
            new QuizAttempt { UserId = user2.Id, QuizId = quiz.Id, IsCompleted = true, TotalScore = 50, CompletedAt = DateTime.UtcNow }
        );
        await _testDb.Context.SaveChangesAsync();

        var result = await _sut.GetLeaderboard(Difficulty.Easy);

        Assert.Equal(2, result.Count);
        Assert.Equal("player1", result[0].Username);
        Assert.Equal("player2", result[1].Username);
        Assert.Equal(1, result[0].Rank);
        Assert.Equal(2, result[1].Rank);
    }

    [Fact]
    public async Task GetLeaderboardFiltersByDifficulty()
    {
        var user = new User { Username = "player", Email = "p@test.com", PasswordHash = "h" };
        var easyQuiz = new Quiz { Title = "Easy", Description = "D", Difficulty = Difficulty.Easy, IsActive = true };
        var hardQuiz = new Quiz { Title = "Hard", Description = "D", Difficulty = Difficulty.Hard, IsActive = true };
        _testDb.Context.Users.Add(user);
        _testDb.Context.Quizzes.AddRange(easyQuiz, hardQuiz);
        await _testDb.Context.SaveChangesAsync();

        _testDb.Context.Questions.AddRange(
            new Question { QuizId = easyQuiz.Id, Text = "Q1", Points = 10, Type = QuestionType.MultipleChoice },
            new Question { QuizId = hardQuiz.Id, Text = "Q2", Points = 20, Type = QuestionType.MultipleChoice }
        );
        await _testDb.Context.SaveChangesAsync();

        _testDb.Context.QuizAttempts.AddRange(
            new QuizAttempt { UserId = user.Id, QuizId = easyQuiz.Id, IsCompleted = true, TotalScore = 10, CompletedAt = DateTime.UtcNow },
            new QuizAttempt { UserId = user.Id, QuizId = hardQuiz.Id, IsCompleted = true, TotalScore = 20, CompletedAt = DateTime.UtcNow }
        );
        await _testDb.Context.SaveChangesAsync();

        var result = await _sut.GetLeaderboard(Difficulty.Easy);

        Assert.Single(result);
        Assert.Equal(10, result[0].BestScore);
    }
}
