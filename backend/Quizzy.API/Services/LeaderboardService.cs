using Microsoft.EntityFrameworkCore;
using Quizzy.API.Data;
using Quizzy.API.DTOs;
using Quizzy.API.Models;

namespace Quizzy.API.Services;

public class LeaderboardService(AppDbContext db)
{
    public async Task<List<LeaderboardEntryDto>> GetLeaderboard(Difficulty difficulty)
    {
        var entries = await db.QuizAttempts
            .Where(a => a.IsCompleted && a.Quiz.Difficulty == difficulty)
            .GroupBy(a => new { a.UserId, a.User.Username })
            .Select(g => new
            {
                g.Key.UserId,
                g.Key.Username,
                BestScore = g.Max(a => a.TotalScore),
                MaxPossibleScore = g.Max(a => a.Quiz.Questions.Sum(q => q.Points)),
                AttemptsCount = g.Count(),
                BestAttemptDate = g.OrderByDescending(a => a.TotalScore).First().CompletedAt ?? g.OrderByDescending(a => a.TotalScore).First().StartedAt
            })
            .OrderByDescending(x => x.BestScore)
            .Take(50)
            .ToListAsync();

        return entries.Select((e, i) => new LeaderboardEntryDto(
            i + 1, e.UserId, e.Username, e.BestScore, e.MaxPossibleScore, e.AttemptsCount, e.BestAttemptDate
        )).ToList();
    }
}
