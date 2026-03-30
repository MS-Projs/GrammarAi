using GrammarAi.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace GrammarAi.Infrastructure.Bot.Handlers;

public class IdleHandler(IAppDbContext db)
{
    public async Task HandleAsync(BotHandlerContext ctx)
    {
        var text = ctx.MessageText?.Trim() ?? string.Empty;

        switch (text.Split(' ')[0])
        {
            case "/start":
                await ctx.Bot.SendMessage(ctx.ChatId,
                    $"👋 Welcome, {ctx.User.DisplayName}!\n\n" +
                    "I help you practice English exercises from images.\n\n" +
                    "📷 Send /upload to add a new exercise\n" +
                    "📚 Send /myexercises to browse your library\n" +
                    "📊 Send /stats to see your progress",
                    cancellationToken: ctx.Ct);
                break;

            case "/upload":
                ctx.Session.Transition("awaiting_image");
                await ctx.Bot.SendMessage(ctx.ChatId,
                    "📷 Send me a photo or document of your exercise worksheet.\n\nYou can send multiple images.",
                    cancellationToken: ctx.Ct);
                break;

            case "/myexercises":
                await ShowExerciseListAsync(ctx);
                break;

            case "/stats":
                await ShowStatsAsync(ctx);
                break;

            case "/help":
                await ctx.Bot.SendMessage(ctx.ChatId,
                    "*Commands:*\n" +
                    "/upload — Upload a new exercise image\n" +
                    "/myexercises — Browse your exercises\n" +
                    "/stats — View your learning stats\n" +
                    "/cancel — Cancel current action",
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                    cancellationToken: ctx.Ct);
                break;

            default:
                await ctx.Bot.SendMessage(ctx.ChatId,
                    "Use /help to see available commands.",
                    cancellationToken: ctx.Ct);
                break;
        }
    }

    private async Task ShowExerciseListAsync(BotHandlerContext ctx)
    {
        var exercises = await db.Exercises
            .Where(e => e.OwnerId == ctx.User.Id && e.Status == Domain.Enums.ExerciseStatus.Ready)
            .OrderByDescending(e => e.CreatedAt)
            .Take(10)
            .ToListAsync(ctx.Ct);

        if (exercises.Count == 0)
        {
            await ctx.Bot.SendMessage(ctx.ChatId,
                "You have no exercises yet. Send /upload to add one!",
                cancellationToken: ctx.Ct);
            return;
        }

        var buttons = exercises.Select(e => new[]
        {
            InlineKeyboardButton.WithCallbackData(
                $"{e.Title ?? "Untitled"} ({e.Difficulty?.ToString() ?? "?"})",
                $"exercise:{e.Id}")
        }).ToList();

        buttons.Add([InlineKeyboardButton.WithCallbackData("📷 Upload New", "cmd:upload")]);

        await ctx.Bot.SendMessage(ctx.ChatId,
            "*Your Exercises:*",
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
            replyMarkup: new InlineKeyboardMarkup(buttons),
            cancellationToken: ctx.Ct);
    }

    private async Task ShowStatsAsync(BotHandlerContext ctx)
    {
        var totalEx = await db.Exercises.CountAsync(e => e.OwnerId == ctx.User.Id, ctx.Ct);
        var totalAns = await db.UserAnswers.CountAsync(a => a.UserId == ctx.User.Id, ctx.Ct);
        var correct = await db.UserAnswers.CountAsync(a => a.UserId == ctx.User.Id && a.IsCorrect == true, ctx.Ct);
        var streak = await db.Streaks.FirstOrDefaultAsync(s => s.UserId == ctx.User.Id, ctx.Ct);
        var accuracy = totalAns > 0 ? Math.Round((double)correct / totalAns * 100, 1) : 0;

        await ctx.Bot.SendMessage(ctx.ChatId,
            $"📊 *Your Stats*\n\n" +
            $"📚 Exercises: {totalEx}\n" +
            $"✅ Questions answered: {totalAns}\n" +
            $"🎯 Accuracy: {accuracy}%\n" +
            $"🔥 Current streak: {streak?.CurrentStreak ?? 0} days\n" +
            $"🏆 Best streak: {streak?.LongestStreak ?? 0} days",
            parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
            cancellationToken: ctx.Ct);
    }
}
