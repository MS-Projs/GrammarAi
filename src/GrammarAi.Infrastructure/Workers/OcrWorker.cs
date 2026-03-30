using System.Text.Json;
using GrammarAi.Application.Common.Interfaces;
using GrammarAi.Domain.Entities;
using GrammarAi.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace GrammarAi.Infrastructure.Workers;

public class OcrWorker(
    IAppDbContext db,
    IAiService ai,
    IStorageService storage,
    IConnectionMultiplexer redis,
    ILogger<OcrWorker> logger)
{
    public async Task ProcessExerciseAsync(Guid exerciseId, CancellationToken ct)
    {
        var job = await db.OcrJobs.FirstOrDefaultAsync(j => j.ExerciseId == exerciseId, ct);
        var exercise = await db.Exercises.FirstOrDefaultAsync(e => e.Id == exerciseId, ct);

        if (exercise is null)
        {
            logger.LogWarning("Exercise {Id} not found for OCR job.", exerciseId);
            return;
        }

        job ??= OcrJob.Create(exerciseId);
        job.Start();
        exercise.MarkProcessing();
        await db.SaveChangesAsync(ct);

        try
        {
            var images = await db.ExerciseImages
                .Where(i => i.ExerciseId == exerciseId)
                .OrderBy(i => i.PageNumber)
                .ToListAsync(ct);

            if (images.Count == 0)
                throw new InvalidOperationException("No images found for exercise.");

            // Step 1: OCR each image
            var ocrParts = new List<string>();
            foreach (var image in images)
            {
                logger.LogInformation("OCR processing page {Page} for exercise {Id}", image.PageNumber, exerciseId);
                var bytes = await storage.DownloadAsync(image.FileKey, ct);
                var text = await ai.ExtractTextFromImageAsync(bytes, ct);
                image.OcrRaw = text;
                ocrParts.Add($"[Page {image.PageNumber}]\n{text}");
            }

            await db.SaveChangesAsync(ct);

            // Step 2: Parse exercise structure
            var combinedText = string.Join("\n\n", ocrParts);
            exercise.SourceText = combinedText;
            var parsed = await ai.ParseExerciseAsync(combinedText, ct);

            // Step 3: Persist questions and answers in transaction
            // Remove old questions if retry
            var existingQuestions = await db.Questions
                .Where(q => q.ExerciseId == exerciseId)
                .ToListAsync(ct);
            foreach (var q in existingQuestions)
                db.Questions.Remove(q);

            foreach (var pq in parsed.Questions)
            {
                var type = Enum.TryParse<ExerciseType>(parsed.ExerciseType, true, out var t)
                    ? t : ExerciseType.MultipleChoice;

                var question = Question.Create(exerciseId, pq.Order, pq.Body, type, pq.Explanation);
                db.Questions.Add(question);

                foreach (var pa in pq.Answers)
                    db.Answers.Add(Answer.Create(question.Id, pq.Answers.IndexOf(pa), pa.Text, pa.IsCorrect));
            }

            // Update exercise metadata
            exercise.Title ??= parsed.Title;
            if (Enum.TryParse<ExerciseType>(parsed.ExerciseType, true, out var exType))
                exercise.ExerciseType = exType;
            if (parsed.Difficulty is not null && Enum.TryParse<DifficultyLevel>(parsed.Difficulty, true, out var diff))
                exercise.Difficulty = diff;

            exercise.Metadata = JsonSerializer.Serialize(new { model = "gpt-4o", parsed_at = DateTimeOffset.UtcNow });
            exercise.MarkReady();
            job.Complete();

            await db.SaveChangesAsync(ct);

            // Step 4: Notify via Redis pub/sub
            var pub = redis.GetSubscriber();
            await pub.PublishAsync(
                RedisChannel.Literal($"exercise:ready:{exerciseId}"),
                JsonSerializer.Serialize(new { exerciseId }));

            logger.LogInformation("Exercise {Id} processed successfully.", exerciseId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "OCR failed for exercise {Id}", exerciseId);
            exercise.MarkFailed(ex.Message);
            job.Fail(ex.ToString());
            await db.SaveChangesAsync(ct);

            // Notify failure
            var pub = redis.GetSubscriber();
            await pub.PublishAsync(
                RedisChannel.Literal($"exercise:failed:{exerciseId}"),
                JsonSerializer.Serialize(new { exerciseId, error = ex.Message }));

            throw; // Let Hangfire handle retry
        }
    }
}
