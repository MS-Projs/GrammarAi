namespace GrammarAi.Domain.Entities;

public class Answer
{
    public Guid Id { get; private set; }
    public Guid QuestionId { get; private set; }
    public int OrderIndex { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public string? Explanation { get; set; }

    public Question Question { get; private set; } = null!;

    private Answer() { }

    public static Answer Create(Guid questionId, int orderIndex, string text, bool isCorrect, string? explanation = null)
        => new()
        {
            Id = Guid.NewGuid(),
            QuestionId = questionId,
            OrderIndex = orderIndex,
            Text = text,
            IsCorrect = isCorrect,
            Explanation = explanation
        };
}
