namespace GrammarAi.Domain.Entities;

public class ExerciseImage
{
    public Guid Id { get; private set; }
    public Guid ExerciseId { get; private set; }
    public string FileKey { get; private set; } = string.Empty;
    public string FileUrl { get; private set; } = string.Empty;
    public string? TelegramFileId { get; set; }
    public string MimeType { get; private set; } = "image/jpeg";
    public int? SizeBytes { get; private set; }
    public int PageNumber { get; private set; }
    public string? OcrRaw { get; set; }
    public DateTimeOffset UploadedAt { get; private set; }

    public Exercise Exercise { get; private set; } = null!;

    private ExerciseImage() { }

    public static ExerciseImage Create(Guid exerciseId, string fileKey, string fileUrl, string mimeType, int? sizeBytes, int pageNumber, string? telegramFileId = null)
        => new()
        {
            Id = Guid.NewGuid(),
            ExerciseId = exerciseId,
            FileKey = fileKey,
            FileUrl = fileUrl,
            MimeType = mimeType,
            SizeBytes = sizeBytes,
            PageNumber = pageNumber,
            TelegramFileId = telegramFileId,
            UploadedAt = DateTimeOffset.UtcNow
        };
}
