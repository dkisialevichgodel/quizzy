using Microsoft.Extensions.Configuration;
using NSubstitute;
using Quizzy.Logic.DTOs;
using Quizzy.Logic.Services;

namespace Quizzy.Logic.Tests;

public class AuthServiceTests : IDisposable
{
    private readonly TestDb _testDb = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        var config = Substitute.For<IConfiguration>();
        config["Jwt:Key"].Returns("ThisIsAVeryLongSecretKeyForTestingPurposesAtLeast32Bytes!");
        config["Jwt:Issuer"].Returns("TestIssuer");
        config["Jwt:Audience"].Returns("TestAudience");
        _sut = new AuthService(_testDb.Context, config);
    }

    public void Dispose() => _testDb.Dispose();

    [Fact]
    public async Task RegisterWhenNewUserThenCreatesUserAndReturnsToken()
    {
        var result = await _sut.Register(new RegisterRequest("testuser", "test@example.com", "Password123!"));

        Assert.NotEmpty(result.Token);
        Assert.Equal("testuser", result.User.Username);
        Assert.Equal("test@example.com", result.User.Email);
        Assert.Equal("User", result.User.Role);
    }

    [Fact]
    public async Task RegisterWhenEmailExistsThenThrows()
    {
        await _sut.Register(new RegisterRequest("user1", "test@example.com", "Password123!"));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.Register(new RegisterRequest("user2", "test@example.com", "Password123!")));

        Assert.Equal("Email already registered.", ex.Message);
    }

    [Fact]
    public async Task RegisterWhenUsernameExistsThenThrows()
    {
        await _sut.Register(new RegisterRequest("testuser", "first@example.com", "Password123!"));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.Register(new RegisterRequest("testuser", "second@example.com", "Password123!")));

        Assert.Equal("Username already taken.", ex.Message);
    }

    [Fact]
    public async Task LoginWhenValidCredentialsThenReturnsToken()
    {
        await _sut.Register(new RegisterRequest("testuser", "test@example.com", "Password123!"));

        var result = await _sut.Login(new LoginRequest("test@example.com", "Password123!"));

        Assert.NotEmpty(result.Token);
        Assert.Equal("testuser", result.User.Username);
    }

    [Fact]
    public async Task LoginWhenEmailNotFoundThenThrows()
    {
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.Login(new LoginRequest("nonexistent@example.com", "Password123!")));

        Assert.Equal("Invalid credentials.", ex.Message);
    }

    [Fact]
    public async Task LoginWhenWrongPasswordThenThrows()
    {
        await _sut.Register(new RegisterRequest("testuser", "test@example.com", "Password123!"));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.Login(new LoginRequest("test@example.com", "WrongPassword!")));

        Assert.Equal("Invalid credentials.", ex.Message);
    }

    [Fact]
    public async Task GetMeWhenUserExistsThenReturnsUser()
    {
        var registered = await _sut.Register(new RegisterRequest("testuser", "test@example.com", "Password123!"));

        var result = await _sut.GetMe(registered.User.Id);

        Assert.Equal("testuser", result.Username);
        Assert.Equal("test@example.com", result.Email);
    }

    [Fact]
    public async Task GetMeWhenUserNotFoundThenThrows()
    {
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _sut.GetMe(999));

        Assert.Equal("User not found.", ex.Message);
    }
}
