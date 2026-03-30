namespace GrammarAi.Domain.Entities;

public class UserAnswer
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid QuestionId { get; private set; }
    public Guid? AnswerId { get; private set; }
    public string? FreeText { get; private set; }
    public bool? IsCorrect { get; set; }
    public decimal Score { get; set; }
    public int? TimeSpentMs { get; private set; }
    public string Platform { get; private set; } = "web";
    public DateTimeOffset AnsweredAt { get; private set; }

    public User User { get; private set; } = null!;
    public Question Question { get; private set; } = null!;
    public Answer? Answer { get; private set; }

    private UserAnswer() { }

    public static UserAnswer Create(Guid userId, Guid questionId, Guid? answerId, string? freeText, string platform, int? timeSpentMs = null)
        => new()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            QuestionId = questionId,
            AnswerId = answerId,
            FreeText = freeText,
            Platform = platform,
            TimeSpentMs = timeSpentMs,
            AnsweredAt = DateTimeOffset.UtcNow
        };
}
