namespace GrammarAi.Domain.Entities;

public class BotSession
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public long ChatId { get; private set; }
    public string State { get; set; } = "idle";
    public string Context { get; set; } = "{}";
    public int? LastMessageId { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public User User { get; private set; } = null!;

    private BotSession() { }

    public static BotSession Create(Guid userId, long chatId)
        => new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            ChatId = chatId,
            State = "idle",
            Context = "{}",
            UpdatedAt = DateTimeOffset.UtcNow
        };

    public void Transition(string newState, string context = "{}")
    {
        State = newState;
        Context = context;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
