using Microsoft.EntityFrameworkCore;
using Quizzy.Data;
using Quizzy.Logic.DTOs;
using Quizzy.Data.Models;

namespace Quizzy.Logic.Services;

public class QuestionService(AppDbContext db)
{
    public async Task<QuestionDto> Create(CreateQuestionRequest request)
    {
        var question = new Question
        {
            QuizId = request.QuizId,
            Text = request.Text,
            Type = request.Type,
            MediaId = request.MediaId,
            OrderIndex = request.OrderIndex,
            Points = request.Points,
            TimeLimitOverride = request.TimeLimitOverride
        };

        db.Questions.Add(question);
        await db.SaveChangesAsync();

        if (request.AnswerOptions?.Count > 0)
        {
            foreach (var opt in request.AnswerOptions)
            {
                db.AnswerOptions.Add(new AnswerOption
                {
                    QuestionId = question.Id,
                    Text = opt.Text,
                    IsCorrect = opt.IsCorrect,
                    MediaId = opt.MediaId
                });
            }
            await db.SaveChangesAsync();
        }

        if (request.OpenTextAnswers?.Count > 0)
        {
            foreach (var ans in request.OpenTextAnswers)
            {
                db.OpenTextAnswers.Add(new OpenTextAnswer
                {
                    QuestionId = question.Id,
                    Text = ans.Text,
                    SimilarityThreshold = ans.SimilarityThreshold
                });
            }
            await db.SaveChangesAsync();
        }

        return await GetById(question.Id);
    }

    public async Task<QuestionDto> Update(int id, UpdateQuestionRequest request)
    {
        var question = await db.Questions
            .Include(q => q.AnswerOptions)
            .Include(q => q.OpenTextAnswers)
            .FirstOrDefaultAsync(q => q.Id == id)
            ?? throw new InvalidOperationException("Question not found.");

        question.Text = request.Text;
        question.Type = request.Type;
        question.MediaId = request.MediaId;
        question.OrderIndex = request.OrderIndex;
        question.Points = request.Points;
        question.TimeLimitOverride = request.TimeLimitOverride;

        // Replace answer options
        db.AnswerOptions.RemoveRange(question.AnswerOptions);
        if (request.AnswerOptions?.Count > 0)
        {
            foreach (var opt in request.AnswerOptions)
            {
                db.AnswerOptions.Add(new AnswerOption
                {
                    QuestionId = question.Id,
                    Text = opt.Text,
                    IsCorrect = opt.IsCorrect,
                    MediaId = opt.MediaId
                });
            }
        }

        // Replace open text answers
        db.OpenTextAnswers.RemoveRange(question.OpenTextAnswers);
        if (request.OpenTextAnswers?.Count > 0)
        {
            foreach (var ans in request.OpenTextAnswers)
            {
                db.OpenTextAnswers.Add(new OpenTextAnswer
                {
                    QuestionId = question.Id,
                    Text = ans.Text,
                    SimilarityThreshold = ans.SimilarityThreshold
                });
            }
        }

        await db.SaveChangesAsync();
        return await GetById(question.Id);
    }

    public async Task Delete(int id)
    {
        var question = await db.Questions.FindAsync(id)
            ?? throw new InvalidOperationException("Question not found.");
        db.Questions.Remove(question);
        await db.SaveChangesAsync();
    }

    private async Task<QuestionDto> GetById(int id)
    {
        var q = await db.Questions
            .Include(q => q.AnswerOptions)
            .Include(q => q.OpenTextAnswers)
            .FirstAsync(q => q.Id == id);

        return new QuestionDto(
            q.Id, q.QuizId, q.Text, q.Type, q.MediaId, q.OrderIndex, q.Points, q.TimeLimitOverride,
            q.AnswerOptions.Select(a => new AnswerOptionDto(a.Id, a.Text, a.IsCorrect, a.MediaId)).ToList(),
            q.OpenTextAnswers.Select(a => new OpenTextAnswerDto(a.Id, a.Text, a.SimilarityThreshold)).ToList()
        );
    }
}
