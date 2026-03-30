using GrammarAi.Domain.Enums;

namespace GrammarAi.Domain.Entities;

public class Exercise
{
    public Guid Id { get; private set; }
    public Guid OwnerId { get; private set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? SourceText { get; set; }
    public ExerciseType ExerciseType { get; set; } = ExerciseType.MultipleChoice;
    public DifficultyLevel? Difficulty { get; set; }
    public ExerciseStatus Status { get; set; } = ExerciseStatus.Pending;
    public string? ErrorMessage { get; set; }
    public bool IsPublic { get; set; }
    public List<string> Tags { get; set; } = [];
    public string Metadata { get; set; } = "{}";
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public User Owner { get; private set; } = null!;
    public ICollection<ExerciseImage> Images { get; private set; } = [];
    public ICollection<Question> Questions { get; private set; } = [];
    public OcrJob? OcrJob { get; private set; }

    private Exercise() { }

    public static Exercise Create(Guid ownerId, string? title, ExerciseType type, DifficultyLevel? difficulty, List<string>? tags, bool isPublic)
        => new()
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId,
            Title = title,
            ExerciseType = type,
            Difficulty = difficulty,
            Tags = tags ?? [],
            IsPublic = isPublic,
            Status = ExerciseStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

    public void MarkProcessing() { Status = ExerciseStatus.Processing; UpdatedAt = DateTimeOffset.UtcNow; }
    public void MarkReady()     { Status = ExerciseStatus.Ready;      UpdatedAt = DateTimeOffset.UtcNow; }
    public void MarkFailed(string error) { Status = ExerciseStatus.Failed; ErrorMessage = error; UpdatedAt = DateTimeOffset.UtcNow; }
}
