using GrammarAi.Application.Common.DTOs;
using GrammarAi.Application.Common.Interfaces;
using GrammarAi.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GrammarAi.Application.Features.Exercises.Commands;

public record ImageUpload(Stream Stream, string FileName, string ContentType, string? TelegramFileId = null);

public record UploadExerciseImagesCommand(
    Guid ExerciseId,
    Guid UserId,
    List<ImageUpload> Images
) : IRequest<List<ImageUploadResultDto>>;

public class UploadExerciseImagesCommandHandler(
    IAppDbContext db,
    IStorageService storage,
    IBackgroundJobService jobs) : IRequestHandler<UploadExerciseImagesCommand, List<ImageUploadResultDto>>
{
    public async Task<List<ImageUploadResultDto>> Handle(UploadExerciseImagesCommand req, CancellationToken ct)
    {
        var exercise = await db.Exercises
            .FirstOrDefaultAsync(e => e.Id == req.ExerciseId && e.OwnerId == req.UserId, ct)
            ?? throw new KeyNotFoundException("Exercise not found.");

        var existingCount = await db.ExerciseImages
            .CountAsync(i => i.ExerciseId == req.ExerciseId, ct);

        var results = new List<ImageUploadResultDto>();

        for (int i = 0; i < req.Images.Count; i++)
        {
            var img = req.Images[i];
            var key = $"exercises/{req.ExerciseId}/{Guid.NewGuid()}{Path.GetExtension(img.FileName)}";
            var (fileKey, fileUrl) = await storage.UploadAsync(img.Stream, key, img.ContentType, ct);

            var image = ExerciseImage.Create(
                req.ExerciseId, fileKey, fileUrl, img.ContentType,
                null, existingCount + i + 1, img.TelegramFileId);

            db.ExerciseImages.Add(image);
            results.Add(new ImageUploadResultDto(image.Id, fileUrl, image.PageNumber));
        }

        // Create OCR job record
        var ocrJob = OcrJob.Create(req.ExerciseId);
        db.OcrJobs.Add(ocrJob);
        exercise.MarkProcessing();

        await db.SaveChangesAsync(ct);

        // Enqueue background job
        var hangfireId = jobs.EnqueueOcrJob(req.ExerciseId);
        ocrJob.Start(hangfireId);
        await db.SaveChangesAsync(ct);

        return results;
    }
}
