using Quizzy.API.Models;

namespace Quizzy.API.DTOs;

// ─── Auth ───
public record RegisterRequest(string Username, string Email, string Password);
public record LoginRequest(string Email, string Password);
public record AuthResponse(string Token, UserDto User);
public record UserDto(int Id, string Username, string Email, string Role);

// ─── Quiz ───
public record QuizListDto(int Id, string Title, string Description, Difficulty Difficulty, int TimeLimitPerQuestion, bool IsActive, int QuestionCount);
public record QuizDetailDto(int Id, string Title, string Description, Difficulty Difficulty, int TimeLimitPerQuestion, bool IsActive, List<QuestionDto> Questions);
public record CreateQuizRequest(string Title, string Description, Difficulty Difficulty, int TimeLimitPerQuestion);
public record UpdateQuizRequest(string Title, string Description, Difficulty Difficulty, int TimeLimitPerQuestion, bool IsActive);

// ─── Question ───
public record QuestionDto(int Id, int QuizId, string Text, QuestionType Type, int? MediaId, int OrderIndex, int Points, int? TimeLimitOverride, List<AnswerOptionDto> AnswerOptions, List<OpenTextAnswerDto> OpenTextAnswers);
public record QuestionPlayDto(int Id, string Text, QuestionType Type, int? MediaId, int Points, int TimeLimit, List<AnswerOptionPlayDto> AnswerOptions);
public record CreateQuestionRequest(int QuizId, string Text, QuestionType Type, int? MediaId, int OrderIndex, int Points, int? TimeLimitOverride, List<CreateAnswerOptionRequest>? AnswerOptions, List<CreateOpenTextAnswerRequest>? OpenTextAnswers);
public record UpdateQuestionRequest(string Text, QuestionType Type, int? MediaId, int OrderIndex, int Points, int? TimeLimitOverride, List<CreateAnswerOptionRequest>? AnswerOptions, List<CreateOpenTextAnswerRequest>? OpenTextAnswers);

// ─── Answer Option ───
public record AnswerOptionDto(int Id, string Text, bool IsCorrect, int? MediaId);
public record AnswerOptionPlayDto(int Id, string Text, int? MediaId);
public record CreateAnswerOptionRequest(string Text, bool IsCorrect, int? MediaId);

// ─── Open Text Answer ───
public record OpenTextAnswerDto(int Id, string Text, double SimilarityThreshold);
public record CreateOpenTextAnswerRequest(string Text, double SimilarityThreshold);

// ─── Attempt ───
public record StartAttemptRequest(int QuizId);
public record SubmitAnswerRequest(int QuestionId, int? SelectedOptionId, string? TextAnswer, int TimeSpent);
public record AttemptResultDto(int Id, int QuizId, string QuizTitle, Difficulty Difficulty, DateTime StartedAt, DateTime? CompletedAt, int BaseScore, int TimeBonus, int TotalScore, int MaxPossibleScore, List<AttemptAnswerDto> Answers);
public record AttemptAnswerDto(int Id, int QuestionId, string QuestionText, QuestionType QuestionType, int? SelectedOptionId, string? SelectedOptionText, string? TextAnswer, bool IsCorrect, int PointsEarned, int TimeSpent, double? SimilarityScore, string? CorrectAnswer);
public record AttemptHistoryDto(int Id, int QuizId, string QuizTitle, Difficulty Difficulty, DateTime StartedAt, DateTime? CompletedAt, int TotalScore, int MaxPossibleScore, bool IsCompleted);

// ─── Leaderboard ───
public record LeaderboardEntryDto(int Rank, int UserId, string Username, int BestScore, int MaxPossibleScore, int AttemptsCount, DateTime BestAttemptDate);
