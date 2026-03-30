using GrammarAi.Domain.Enums;

namespace GrammarAi.Domain.Entities;

public class OcrJob
{
    public Guid Id { get; private set; }
    public Guid ExerciseId { get; private set; }
    public string? HangfireJobId { get; set; }
    public JobStatus Status { get; set; } = JobStatus.Queued;
    public int AttemptCount { get; set; }
    public string? ErrorDetails { get; set; }
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? FinishedAt { get; set; }
    public DateTimeOffset CreatedAt { get; private set; }

    public Exercise Exercise { get; private set; } = null!;

    private OcrJob() { }

    public static OcrJob Create(Guid exerciseId)
        => new()
        {
            Id = Guid.NewGuid(),
            ExerciseId = exerciseId,
            Status = JobStatus.Queued,
            CreatedAt = DateTimeOffset.UtcNow
        };

    public void Start(string? hangfireJobId = null)
    {
        Status = JobStatus.Processing;
        StartedAt = DateTimeOffset.UtcNow;
        AttemptCount++;
        if (hangfireJobId is not null) HangfireJobId = hangfireJobId;
    }

    public void Complete()
    {
        Status = JobStatus.Done;
        FinishedAt = DateTimeOffset.UtcNow;
    }

    public void Fail(string errorDetails)
    {
        Status = JobStatus.Failed;
        ErrorDetails = errorDetails;
        FinishedAt = DateTimeOffset.UtcNow;
    }
}
