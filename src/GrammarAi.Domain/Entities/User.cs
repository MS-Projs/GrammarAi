namespace GrammarAi.Domain.Entities;

public class User
{
    public Guid Id { get; private set; }
    public long? TelegramId { get; private set; }
    public string? WebEmail { get; private set; }
    public string DisplayName { get; private set; } = "Learner";
    public string? AvatarUrl { get; private set; }
    public string LanguageCode { get; private set; } = "en";
    public string Timezone { get; private set; } = "UTC";
    public bool IsPremium { get; private set; }
    public bool IsBlocked { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public ICollection<Exercise> Exercises { get; private set; } = [];
    public ICollection<AuthToken> AuthTokens { get; private set; } = [];
    public ICollection<UserAnswer> UserAnswers { get; private set; } = [];
    public Streak? Streak { get; private set; }

    private User() { }

    public static User Create(long? telegramId, string? webEmail, string displayName)
    {
        if (telegramId is null && webEmail is null)
            throw new ArgumentException("Either telegramId or webEmail must be provided.");

        return new User
        {
            Id = Guid.NewGuid(),
            TelegramId = telegramId,
            WebEmail = webEmail,
            DisplayName = displayName,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
    }

    public void UpdateProfile(string displayName, string? timezone, string? languageCode)
    {
        DisplayName = displayName;
        if (timezone is not null) Timezone = timezone;
        if (languageCode is not null) LanguageCode = languageCode;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Block() { IsBlocked = true; UpdatedAt = DateTimeOffset.UtcNow; }
    public void Unblock() { IsBlocked = false; UpdatedAt = DateTimeOffset.UtcNow; }
}
