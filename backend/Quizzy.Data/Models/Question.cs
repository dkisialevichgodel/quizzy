namespace Quizzy.Data.Models;

public class Question
{
    public int Id { get; set; }
    public int QuizId { get; set; }
    public string Text { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public int? MediaId { get; set; }
    public int OrderIndex { get; set; }
    public int Points { get; set; } = 10;
    public int? TimeLimitOverride { get; set; }

    public Quiz Quiz { get; set; } = null!;
    public MediaFile? Media { get; set; }
    public ICollection<AnswerOption> AnswerOptions { get; set; } = [];
    public ICollection<OpenTextAnswer> OpenTextAnswers { get; set; } = [];
}
