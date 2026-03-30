using System.Text.Json;
using GrammarAi.Application.Common.Interfaces;
using GrammarAi.Application.Features.Exercises.Commands;
using GrammarAi.Domain.Entities;
using GrammarAi.Domain.Enums;
using MediatR;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GrammarAi.Infrastructure.Bot.Handlers;

public class AwaitingImageHandler(
    IAppDbContext db,
    IStorageService storage,
    IBackgroundJobService jobs,
    IMediator mediator)
{
    public async Task HandleAsync(BotHandlerContext ctx)
    {
        string? fileId = null;
        string mimeType = "image/jpeg";

        if (ctx.Photos?.Length > 0)
        {
            // Pick largest photo size
            fileId = ctx.Photos.MaxBy(p => p.FileSize)?.FileId;
            mimeType = "image/jpeg";
        }
        else if (ctx.Document is not null)
        {
            var doc = ctx.Document;
            if (doc.MimeType?.StartsWith("image/") != true && doc.MimeType != "application/pdf")
            {
                await ctx.Bot.SendMessage(ctx.ChatId, "Please send an image or PDF file.", cancellationToken: ctx.Ct);
                return;
            }
            fileId = doc.FileId;
            mimeType = doc.MimeType ?? "image/jpeg";
        }
        else
        {
            await ctx.Bot.SendMessage(ctx.ChatId, "Please send a photo or document.", cancellationToken: ctx.Ct);
            return;
        }

        var processingMsg = await ctx.Bot.SendMessage(ctx.ChatId,
            "⏳ Uploading and processing your exercise...",
            cancellationToken: ctx.Ct);

        try
        {
            // Download from Telegram
            var file = await ctx.Bot.GetFile(fileId!, ctx.Ct);
            using var ms = new MemoryStream();
            await ctx.Bot.DownloadFile(file.FilePath!, ms, ctx.Ct);
            ms.Position = 0;

            // Create exercise
            var exerciseId = await mediator.Send(
                new CreateExerciseCommand(ctx.User.Id, null, null, null, null, null, false), ctx.Ct);

            // Upload image and enqueue OCR
            var ext = Path.GetExtension(file.FilePath ?? ".jpg");
            var uploadResult = await mediator.Send(new UploadExerciseImagesCommand(
                exerciseId,
                ctx.User.Id,
                [new ImageUpload(ms, $"image{ext}", mimeType, fileId)]), ctx.Ct);

            // Transition to processing state
            ctx.Session.Transition("processing_exercise",
                JsonSerializer.Serialize(new { exerciseId }));

            await ctx.Bot.EditMessageText(ctx.ChatId, processingMsg.MessageId,
                "🔍 Analyzing your exercise with AI...\nI'll notify you when it's ready!",
                cancellationToken: ctx.Ct);
        }
        catch (Exception)
        {
            ctx.Session.Transition("idle");
            await ctx.Bot.EditMessageText(ctx.ChatId, processingMsg.MessageId,
                "❌ Failed to process the image. Please try again with /upload",
                cancellationToken: ctx.Ct);
        }
    }
}
