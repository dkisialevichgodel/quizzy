namespace Quizzy.Data.Models;

public class QuizAttempt
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int QuizId { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public int BaseScore { get; set; }
    public int TimeBonus { get; set; }
    public int TotalScore { get; set; }
    public bool IsCompleted { get; set; }

    public User User { get; set; } = null!;
    public Quiz Quiz { get; set; } = null!;
    public ICollection<AttemptAnswer> Answers { get; set; } = [];
}
