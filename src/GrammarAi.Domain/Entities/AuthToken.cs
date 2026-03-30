namespace GrammarAi.Domain.Entities;

public class AuthToken
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }

    public User User { get; private set; } = null!;

    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
    public bool IsValid => !IsRevoked && !IsExpired;

    private AuthToken() { }

    public static AuthToken Create(Guid userId, string tokenHash, DateTimeOffset expiresAt)
        => new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = expiresAt,
            CreatedAt = DateTimeOffset.UtcNow
        };

    public void Revoke() => RevokedAt = DateTimeOffset.UtcNow;
}
