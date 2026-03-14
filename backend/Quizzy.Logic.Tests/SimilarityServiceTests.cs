using Quizzy.Logic.Services;

namespace Quizzy.Logic.Tests;

public class SimilarityServiceTests
{
    private readonly SimilarityService _sut = new();

    [Fact]
    public void WhenBothStringsAreIdenticalThenReturnsOne()
    {
        var result = _sut.ComputeSimilarity("hello", "hello");

        Assert.Equal(1.0, result);
    }

    [Fact]
    public void WhenUserAnswerIsEmptyThenReturnsZero()
    {
        var result = _sut.ComputeSimilarity("", "hello");

        Assert.Equal(0.0, result);
    }

    [Fact]
    public void WhenReferenceAnswerIsEmptyThenReturnsZero()
    {
        var result = _sut.ComputeSimilarity("hello", "");

        Assert.Equal(0.0, result);
    }

    [Fact]
    public void WhenBothStringsAreWhitespaceThenReturnsZero()
    {
        var result = _sut.ComputeSimilarity("  ", "  ");

        Assert.Equal(0.0, result);
    }

    [Fact]
    public void WhenStringsAreIdenticalExceptCaseThenReturnsOne()
    {
        var result = _sut.ComputeSimilarity("Hello World", "hello world");

        Assert.Equal(1.0, result);
    }

    [Fact]
    public void WhenStringsAreIdenticalExceptPunctuationThenReturnsOne()
    {
        var result = _sut.ComputeSimilarity("hello, world!", "hello world");

        Assert.Equal(1.0, result);
    }

    [Fact]
    public void WhenStringsAreSimilarThenReturnsHighScore()
    {
        var result = _sut.ComputeSimilarity("photosynthesis", "photo synthesis");

        Assert.True(result > 0.7);
    }

    [Fact]
    public void WhenStringsAreCompletelyDifferentThenReturnsLowScore()
    {
        var result = _sut.ComputeSimilarity("apple", "quantum mechanics");

        Assert.True(result < 0.5);
    }
}
