using GrammarAi.Domain.Enums;

namespace GrammarAi.Domain.Entities;

public class Question
{
    public Guid Id { get; private set; }
    public Guid ExerciseId { get; private set; }
    public int OrderIndex { get; set; }
    public string Body { get; set; } = string.Empty;
    public string? Explanation { get; set; }
    public ExerciseType ExerciseType { get; set; }
    public int MaxScore { get; set; } = 1;
    public string Metadata { get; set; } = "{}";

    public Exercise Exercise { get; private set; } = null!;
    public ICollection<Answer> Answers { get; private set; } = [];
    public ICollection<UserAnswer> UserAnswers { get; private set; } = [];

    private Question() { }

    public static Question Create(Guid exerciseId, int orderIndex, string body, ExerciseType type, string? explanation = null)
        => new()
        {
            Id = Guid.NewGuid(),
            ExerciseId = exerciseId,
            OrderIndex = orderIndex,
            Body = body,
            ExerciseType = type,
            Explanation = explanation
        };
}
