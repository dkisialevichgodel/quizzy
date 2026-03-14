using F23.StringSimilarity;

namespace Quizzy.API.Services;

public class SimilarityService
{
    private readonly Cosine _cosine = new(2);
    private readonly JaroWinkler _jaroWinkler = new();

    /// <summary>
    /// Computes a combined similarity score between the user answer and a reference answer.
    /// Uses a weighted blend of Cosine similarity (for word overlap) and Jaro-Winkler (for character-level).
    /// Returns 0.0–1.0.
    /// </summary>
    public double ComputeSimilarity(string userAnswer, string referenceAnswer)
    {
        var normalizedUser = Normalize(userAnswer);
        var normalizedRef = Normalize(referenceAnswer);

        if (string.IsNullOrWhiteSpace(normalizedUser) || string.IsNullOrWhiteSpace(normalizedRef))
            return 0.0;

        if (normalizedUser == normalizedRef)
            return 1.0;

        var cosineSim = _cosine.Similarity(normalizedUser, normalizedRef);
        var jaroSim = _jaroWinkler.Similarity(normalizedUser, normalizedRef);

        // Weighted blend: 60% cosine (semantic/word overlap) + 40% Jaro-Winkler (character-level)
        return cosineSim * 0.6 + jaroSim * 0.4;
    }

    private static string Normalize(string input)
    {
        return input.Trim().ToLowerInvariant()
            .Replace(",", " ")
            .Replace(".", " ")
            .Replace("!", " ")
            .Replace("?", " ")
            .Replace("  ", " ")
            .Trim();
    }
}
