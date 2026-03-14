using Microsoft.EntityFrameworkCore;
using Quizzy.API.Data;
using Quizzy.API.DTOs;
using Quizzy.API.Models;

namespace Quizzy.API.Services;

public class QuizService(AppDbContext db)
{
    public async Task<List<QuizListDto>> GetAll()
    {
        return await db.Quizzes
            .Select(q => new QuizListDto(q.Id, q.Title, q.Description, q.Difficulty, q.TimeLimitPerQuestion, q.IsActive, q.Questions.Count))
            .ToListAsync();
    }

    public async Task<List<QuizListDto>> GetActive()
    {
        return await db.Quizzes.Where(q => q.IsActive)
            .Select(q => new QuizListDto(q.Id, q.Title, q.Description, q.Difficulty, q.TimeLimitPerQuestion, q.IsActive, q.Questions.Count))
            .ToListAsync();
    }

    public async Task<QuizDetailDto> GetById(int id, bool includeCorrectAnswers)
    {
        var quiz = await db.Quizzes
            .Include(q => q.Questions.OrderBy(qu => qu.OrderIndex))
                .ThenInclude(q => q.AnswerOptions)
            .Include(q => q.Questions)
                .ThenInclude(q => q.OpenTextAnswers)
            .FirstOrDefaultAsync(q => q.Id == id)
            ?? throw new InvalidOperationException("Quiz not found.");

        return new QuizDetailDto(
            quiz.Id, quiz.Title, quiz.Description, quiz.Difficulty, quiz.TimeLimitPerQuestion, quiz.IsActive,
            quiz.Questions.Select(q => new QuestionDto(
                q.Id, q.QuizId, q.Text, q.Type, q.MediaId, q.OrderIndex, q.Points, q.TimeLimitOverride,
                q.AnswerOptions.Select(a => new AnswerOptionDto(a.Id, a.Text, includeCorrectAnswers && a.IsCorrect, a.MediaId)).ToList(),
                includeCorrectAnswers ? q.OpenTextAnswers.Select(a => new OpenTextAnswerDto(a.Id, a.Text, a.SimilarityThreshold)).ToList() : []
            )).ToList()
        );
    }

    public async Task<List<QuestionPlayDto>> GetQuestionsForPlay(int quizId)
    {
        var quiz = await db.Quizzes.FindAsync(quizId)
            ?? throw new InvalidOperationException("Quiz not found.");

        return await db.Questions
            .Where(q => q.QuizId == quizId)
            .OrderBy(q => q.OrderIndex)
            .Select(q => new QuestionPlayDto(
                q.Id, q.Text, q.Type, q.MediaId, q.Points,
                q.TimeLimitOverride ?? quiz.TimeLimitPerQuestion,
                q.AnswerOptions.Select(a => new AnswerOptionPlayDto(a.Id, a.Text, a.MediaId)).ToList()
            ))
            .ToListAsync();
    }

    public async Task<QuizListDto> Create(CreateQuizRequest request)
    {
        var quiz = new Quiz
        {
            Title = request.Title,
            Description = request.Description,
            Difficulty = request.Difficulty,
            TimeLimitPerQuestion = request.TimeLimitPerQuestion
        };

        db.Quizzes.Add(quiz);
        await db.SaveChangesAsync();

        return new QuizListDto(quiz.Id, quiz.Title, quiz.Description, quiz.Difficulty, quiz.TimeLimitPerQuestion, quiz.IsActive, 0);
    }

    public async Task<QuizListDto> Update(int id, UpdateQuizRequest request)
    {
        var quiz = await db.Quizzes.Include(q => q.Questions).FirstOrDefaultAsync(q => q.Id == id)
            ?? throw new InvalidOperationException("Quiz not found.");

        quiz.Title = request.Title;
        quiz.Description = request.Description;
        quiz.Difficulty = request.Difficulty;
        quiz.TimeLimitPerQuestion = request.TimeLimitPerQuestion;
        quiz.IsActive = request.IsActive;

        await db.SaveChangesAsync();

        return new QuizListDto(quiz.Id, quiz.Title, quiz.Description, quiz.Difficulty, quiz.TimeLimitPerQuestion, quiz.IsActive, quiz.Questions.Count);
    }

    public async Task Delete(int id)
    {
        var quiz = await db.Quizzes.FindAsync(id)
            ?? throw new InvalidOperationException("Quiz not found.");
        db.Quizzes.Remove(quiz);
        await db.SaveChangesAsync();
    }
}
