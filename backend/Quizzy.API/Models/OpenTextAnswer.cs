namespace Quizzy.API.Models;

public class OpenTextAnswer
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public double SimilarityThreshold { get; set; } = 0.7;

    public Question Question { get; set; } = null!;
}
