using System.Text.Json;
using GrammarAi.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace GrammarAi.Infrastructure.Bot;

/// <summary>
/// Subscribes to Redis pub/sub events and notifies Telegram users when OCR jobs complete.
/// </summary>
public class BotNotificationService(
    IConnectionMultiplexer redis,
    ITelegramBotClient bot,
    IAppDbContext db,
    ILogger<BotNotificationService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var sub = redis.GetSubscriber();

        await sub.SubscribeAsync(RedisChannel.Pattern("exercise:*"), async (channel, message) =>
        {
            try
            {
                var channelStr = channel.ToString();
                var payload = JsonSerializer.Deserialize<JsonElement>(message.ToString());
                var exerciseIdStr = payload.GetProperty("exerciseId").GetString();
                if (!Guid.TryParse(exerciseIdStr, out var exerciseId)) return;

                var session = await db.BotSessions
                    .FirstOrDefaultAsync(s => s.Context.Contains(exerciseId.ToString())
                        && s.State == "processing_exercise", stoppingToken);

                if (session is null) return;

                if (channelStr.StartsWith("exercise:ready:"))
                {
                    var exercise = await db.Exercises.FindAsync([exerciseId], stoppingToken);
                    var keyboard = new InlineKeyboardMarkup(new[]
                    {
                        new[] { InlineKeyboardButton.WithCallbackData("▶️ Solve Now", $"solve:{exerciseId}") },
                        new[] { InlineKeyboardButton.WithCallbackData("📚 My Exercises", "cmd:myexercises") }
                    });

                    session.Transition("idle");
                    await db.SaveChangesAsync(stoppingToken);

                    await bot.SendMessage(session.ChatId,
                        $"✅ *Exercise ready!*\n\n📖 {exercise?.Title ?? "Your exercise"}\n🎯 Difficulty: {exercise?.Difficulty?.ToString() ?? "Unknown"}",
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                        replyMarkup: keyboard,
                        cancellationToken: stoppingToken);
                }
                else if (channelStr.StartsWith("exercise:failed:"))
                {
                    var error = payload.TryGetProperty("error", out var e) ? e.GetString() : "Unknown error";
                    session.Transition("idle");
                    await db.SaveChangesAsync(stoppingToken);

                    await bot.SendMessage(session.ChatId,
                        $"❌ Failed to process exercise.\n\n`{error}`\n\nTry /upload again.",
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                        cancellationToken: stoppingToken);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error handling Redis notification");
            }
        });

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }
}
