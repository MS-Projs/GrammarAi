namespace GrammarAi.Domain.Entities;

public class Streak
{
    public Guid UserId { get; private set; }
    public int CurrentStreak { get; private set; }
    public int LongestStreak { get; private set; }
    public DateOnly LastActiveDate { get; private set; }
    public DateTimeOffset UpdatedAt { get; private set; }

    public User User { get; private set; } = null!;

    private Streak() { }

    public static Streak Create(Guid userId)
        => new()
        {
            UserId = userId,
            CurrentStreak = 0,
            LongestStreak = 0,
            LastActiveDate = DateOnly.FromDateTime(DateTime.UtcNow),
            UpdatedAt = DateTimeOffset.UtcNow
        };

    public void RecordActivity()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (LastActiveDate == today) return;

        var yesterday = today.AddDays(-1);
        CurrentStreak = LastActiveDate == yesterday ? CurrentStreak + 1 : 1;
        if (CurrentStreak > LongestStreak) LongestStreak = CurrentStreak;
        LastActiveDate = today;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
