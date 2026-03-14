using Microsoft.EntityFrameworkCore;
using Quizzy.Data;
using Quizzy.Logic.DTOs;
using Quizzy.Data.Models;

namespace Quizzy.Logic.Services;

public class AttemptService(AppDbContext db, SimilarityService similarity)
{
    public async Task<AttemptResultDto> StartAttempt(int userId, StartAttemptRequest request)
    {
        var quiz = await db.Quizzes.Include(q => q.Questions).FirstOrDefaultAsync(q => q.Id == request.QuizId)
            ?? throw new InvalidOperationException("Quiz not found.");

        if (!quiz.IsActive)
            throw new InvalidOperationException("Quiz is not active.");

        var attempt = new QuizAttempt
        {
            UserId = userId,
            QuizId = quiz.Id,
            StartedAt = DateTime.UtcNow
        };

        db.QuizAttempts.Add(attempt);
        await db.SaveChangesAsync();

        return MapToResult(attempt, quiz.Title, quiz.Difficulty, quiz.Questions.Sum(q => q.Points));
    }

    public async Task<AttemptAnswerDto> SubmitAnswer(int userId, int attemptId, SubmitAnswerRequest request)
    {
        var attempt = await db.QuizAttempts.FirstOrDefaultAsync(a => a.Id == attemptId && a.UserId == userId)
            ?? throw new InvalidOperationException("Attempt not found.");

        if (attempt.IsCompleted)
            throw new InvalidOperationException("Attempt already completed.");

        // Check for duplicate answer
        if (await db.AttemptAnswers.AnyAsync(a => a.AttemptId == attemptId && a.QuestionId == request.QuestionId))
            throw new InvalidOperationException("Question already answered.");

        var question = await db.Questions
            .Include(q => q.AnswerOptions)
            .Include(q => q.OpenTextAnswers)
            .FirstOrDefaultAsync(q => q.Id == request.QuestionId && q.QuizId == attempt.QuizId)
            ?? throw new InvalidOperationException("Question not found in this quiz.");

        var answer = new AttemptAnswer
        {
            AttemptId = attemptId,
            QuestionId = request.QuestionId,
            SelectedOptionId = request.SelectedOptionId,
            TextAnswer = request.TextAnswer,
            TimeSpent = request.TimeSpent
        };

        // Evaluate answer
        if (question.Type == QuestionType.MultipleChoice)
        {
            var correctOption = question.AnswerOptions.FirstOrDefault(a => a.IsCorrect);
            answer.IsCorrect = request.SelectedOptionId.HasValue && correctOption?.Id == request.SelectedOptionId.Value;
            answer.PointsEarned = answer.IsCorrect ? question.Points : 0;
        }
        else // OpenText
        {
            if (!string.IsNullOrWhiteSpace(request.TextAnswer) && question.OpenTextAnswers.Count > 0)
            {
                var bestSimilarity = question.OpenTextAnswers
                    .Select(refAnswer => new
                    {
                        Score = similarity.ComputeSimilarity(request.TextAnswer!, refAnswer.Text),
                        Threshold = refAnswer.SimilarityThreshold
                    })
                    .OrderByDescending(x => x.Score)
                    .First();

                answer.SimilarityScore = bestSimilarity.Score;
                if (bestSimilarity.Score >= bestSimilarity.Threshold)
                {
                    answer.PointsEarned = (int)Math.Floor(question.Points * bestSimilarity.Score);
                    answer.IsCorrect = bestSimilarity.Score >= 0.9;
                }
            }
        }

        db.AttemptAnswers.Add(answer);
        await db.SaveChangesAsync();

        var correctAnswerText = question.Type == QuestionType.MultipleChoice
            ? question.AnswerOptions.FirstOrDefault(a => a.IsCorrect)?.Text
            : question.OpenTextAnswers.FirstOrDefault()?.Text;

        var selectedOptionText = request.SelectedOptionId.HasValue
            ? question.AnswerOptions.FirstOrDefault(a => a.Id == request.SelectedOptionId.Value)?.Text
            : null;

        return new AttemptAnswerDto(
            answer.Id, answer.QuestionId, question.Text, question.Type,
            answer.SelectedOptionId, selectedOptionText, answer.TextAnswer,
            answer.IsCorrect, answer.PointsEarned, answer.TimeSpent,
            answer.SimilarityScore, correctAnswerText);
    }

    public async Task<AttemptResultDto> CompleteAttempt(int userId, int attemptId)
    {
        var attempt = await db.QuizAttempts
            .Include(a => a.Answers)
                .ThenInclude(a => a.Question)
            .FirstOrDefaultAsync(a => a.Id == attemptId && a.UserId == userId)
            ?? throw new InvalidOperationException("Attempt not found.");

        if (attempt.IsCompleted)
            throw new InvalidOperationException("Attempt already completed.");

        var quiz = await db.Quizzes.Include(q => q.Questions).FirstAsync(q => q.Id == attempt.QuizId);

        attempt.IsCompleted = true;
        attempt.CompletedAt = DateTime.UtcNow;
        attempt.BaseScore = attempt.Answers.Sum(a => a.PointsEarned);

        // Calculate time bonus
        foreach (var answer in attempt.Answers)
        {
            var timeLimit = answer.Question.TimeLimitOverride ?? quiz.TimeLimitPerQuestion;
            if (answer.PointsEarned > 0 && answer.TimeSpent < timeLimit)
            {
                var bonus = (int)Math.Floor((double)(timeLimit - answer.TimeSpent) / timeLimit * 5);
                attempt.TimeBonus += Math.Max(0, bonus);
            }
        }

        attempt.TotalScore = attempt.BaseScore + attempt.TimeBonus;
        await db.SaveChangesAsync();

        return await GetAttemptResult(userId, attemptId);
    }

    public async Task<AttemptResultDto> GetAttemptResult(int userId, int attemptId)
    {
        var attempt = await db.QuizAttempts
            .Include(a => a.Answers).ThenInclude(a => a.Question).ThenInclude(q => q.AnswerOptions)
            .Include(a => a.Answers).ThenInclude(a => a.Question).ThenInclude(q => q.OpenTextAnswers)
            .Include(a => a.Answers).ThenInclude(a => a.SelectedOption)
            .Include(a => a.Quiz)
            .FirstOrDefaultAsync(a => a.Id == attemptId && a.UserId == userId)
            ?? throw new InvalidOperationException("Attempt not found.");

        var maxScore = attempt.Quiz.Questions.Count > 0
            ? await db.Questions.Where(q => q.QuizId == attempt.QuizId).SumAsync(q => q.Points)
            : 0;

        return MapToResult(attempt, attempt.Quiz.Title, attempt.Quiz.Difficulty, maxScore);
    }

    public async Task<List<AttemptHistoryDto>> GetHistory(int userId)
    {
        return await db.QuizAttempts
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.StartedAt)
            .Select(a => new AttemptHistoryDto(
                a.Id, a.QuizId, a.Quiz.Title, a.Quiz.Difficulty,
                a.StartedAt, a.CompletedAt, a.TotalScore,
                a.Quiz.Questions.Sum(q => q.Points),
                a.IsCompleted))
            .ToListAsync();
    }

    private static AttemptResultDto MapToResult(QuizAttempt attempt, string quizTitle, Difficulty difficulty, int maxScore)
    {
        return new AttemptResultDto(
            attempt.Id, attempt.QuizId, quizTitle, difficulty,
            attempt.StartedAt, attempt.CompletedAt,
            attempt.BaseScore, attempt.TimeBonus, attempt.TotalScore, maxScore,
            attempt.Answers.Select(a =>
            {
                var correctAnswer = a.Question.Type == QuestionType.MultipleChoice
                    ? a.Question.AnswerOptions.FirstOrDefault(o => o.IsCorrect)?.Text
                    : a.Question.OpenTextAnswers.FirstOrDefault()?.Text;

                return new AttemptAnswerDto(
                    a.Id, a.QuestionId, a.Question.Text, a.Question.Type,
                    a.SelectedOptionId, a.SelectedOption?.Text, a.TextAnswer,
                    a.IsCorrect, a.PointsEarned, a.TimeSpent,
                    a.SimilarityScore, correctAnswer);
            }).ToList()
        );
    }
}
