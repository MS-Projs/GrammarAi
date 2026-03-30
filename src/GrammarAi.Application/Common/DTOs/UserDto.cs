namespace GrammarAi.Application.Common.DTOs;

public record UserDto(
    Guid Id,
    string DisplayName,
    string? AvatarUrl,
    long? TelegramId,
    string? WebEmail,
    bool IsPremium,
    DateTimeOffset CreatedAt
);

public record UserStatsDto(
    int TotalExercises,
    int TotalQuestions,
    int CorrectAnswers,
    decimal AccuracyPercent,
    int CurrentStreak,
    int LongestStreak
);

public record AuthResultDto(string AccessToken, string RefreshToken, UserDto User);
