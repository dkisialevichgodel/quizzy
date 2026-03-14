using Quizzy.Data.Models;
using Quizzy.Logic.DTOs;
using Quizzy.Logic.Services;

namespace Quizzy.Logic.Tests;

public class QuestionServiceTests : IDisposable
{
    private readonly TestDb _testDb = new();
    private readonly QuestionService _sut;

    public QuestionServiceTests()
    {
        _sut = new QuestionService(_testDb.Context);
    }

    public void Dispose() => _testDb.Dispose();

    private async Task<Quiz> SeedQuiz()
    {
        var quiz = new Quiz { Title = "Quiz", Description = "Desc" };
        _testDb.Context.Quizzes.Add(quiz);
        await _testDb.Context.SaveChangesAsync();
        return quiz;
    }

    [Fact]
    public async Task CreateWithAnswerOptionsThenReturnsDtoWithOptions()
    {
        var quiz = await SeedQuiz();
        var request = new CreateQuestionRequest(
            quiz.Id, "What is 2+2?", QuestionType.MultipleChoice, null, 0, 10, null,
            [new CreateAnswerOptionRequest("4", true, null), new CreateAnswerOptionRequest("5", false, null)],
            null);

        var result = await _sut.Create(request);

        Assert.Equal("What is 2+2?", result.Text);
        Assert.Equal(2, result.AnswerOptions.Count);
        Assert.Single(result.AnswerOptions, a => a.IsCorrect);
    }

    [Fact]
    public async Task CreateWithOpenTextAnswersThenReturnsDtoWithAnswers()
    {
        var quiz = await SeedQuiz();
        var request = new CreateQuestionRequest(
            quiz.Id, "What is photosynthesis?", QuestionType.OpenText, null, 0, 10, null,
            null,
            [new CreateOpenTextAnswerRequest("The process by which plants convert sunlight to energy", 0.7)]);

        var result = await _sut.Create(request);

        Assert.Single(result.OpenTextAnswers);
        Assert.Equal(0.7, result.OpenTextAnswers[0].SimilarityThreshold);
    }

    [Fact]
    public async Task CreateWithNoOptionsThenReturnsDtoWithEmptyLists()
    {
        var quiz = await SeedQuiz();
        var request = new CreateQuestionRequest(
            quiz.Id, "Question", QuestionType.MultipleChoice, null, 0, 10, null, null, null);

        var result = await _sut.Create(request);

        Assert.Empty(result.AnswerOptions);
        Assert.Empty(result.OpenTextAnswers);
    }

    [Fact]
    public async Task UpdateReplacesAnswerOptions()
    {
        var quiz = await SeedQuiz();
        var created = await _sut.Create(new CreateQuestionRequest(
            quiz.Id, "Q", QuestionType.MultipleChoice, null, 0, 10, null,
            [new CreateAnswerOptionRequest("Old", true, null)], null));

        var result = await _sut.Update(created.Id, new UpdateQuestionRequest(
            "Updated Q", QuestionType.MultipleChoice, null, 0, 20, null,
            [new CreateAnswerOptionRequest("New A", false, null), new CreateAnswerOptionRequest("New B", true, null)],
            null));

        Assert.Equal("Updated Q", result.Text);
        Assert.Equal(20, result.Points);
        Assert.Equal(2, result.AnswerOptions.Count);
        Assert.DoesNotContain(result.AnswerOptions, a => a.Text == "Old");
    }

    [Fact]
    public async Task UpdateReplacesOpenTextAnswers()
    {
        var quiz = await SeedQuiz();
        var created = await _sut.Create(new CreateQuestionRequest(
            quiz.Id, "Q", QuestionType.OpenText, null, 0, 10, null,
            null, [new CreateOpenTextAnswerRequest("Old answer", 0.7)]));

        var result = await _sut.Update(created.Id, new UpdateQuestionRequest(
            "Updated Q", QuestionType.OpenText, null, 0, 10, null,
            null, [new CreateOpenTextAnswerRequest("New answer", 0.8)]));

        Assert.Single(result.OpenTextAnswers);
        Assert.Equal("New answer", result.OpenTextAnswers[0].Text);
        Assert.Equal(0.8, result.OpenTextAnswers[0].SimilarityThreshold);
    }

    [Fact]
    public async Task UpdateWhenQuestionNotFoundThenThrows()
    {
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.Update(999, new UpdateQuestionRequest(
                "Q", QuestionType.MultipleChoice, null, 0, 10, null, null, null)));

        Assert.Equal("Question not found.", ex.Message);
    }

    [Fact]
    public async Task DeleteWhenQuestionExistsThenRemovesQuestion()
    {
        var quiz = await SeedQuiz();
        var created = await _sut.Create(new CreateQuestionRequest(
            quiz.Id, "Q", QuestionType.MultipleChoice, null, 0, 10, null, null, null));

        await _sut.Delete(created.Id);

        Assert.Empty(_testDb.Context.Questions);
    }

    [Fact]
    public async Task DeleteWhenQuestionNotFoundThenThrows()
    {
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.Delete(999));

        Assert.Equal("Question not found.", ex.Message);
    }
}
