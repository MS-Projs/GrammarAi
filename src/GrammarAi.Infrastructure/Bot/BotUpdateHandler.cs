using GrammarAi.Application.Common.Interfaces;
using GrammarAi.Domain.Entities;
using GrammarAi.Infrastructure.Bot.Handlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GrammarAi.Infrastructure.Bot;

public class BotUpdateHandler(
    IAppDbContext db,
    ITelegramBotClient bot,
    IdleHandler idleHandler,
    AwaitingImageHandler awaitingImageHandler,
    SolvingExerciseHandler solvingExerciseHandler,
    ShowingResultHandler showingResultHandler,
    ILogger<BotUpdateHandler> logger)
{
    public async Task HandleUpdateAsync(Update update, CancellationToken ct)
    {
        var chatId = update.Message?.Chat.Id ?? update.CallbackQuery?.Message?.Chat.Id;
        var telegramUserId = update.Message?.From?.Id ?? update.CallbackQuery?.From?.Id;

        if (chatId is null || telegramUserId is null) return;

        // Resolve or create user
        var user = await db.Users.FirstOrDefaultAsync(u => u.TelegramId == telegramUserId, ct);
        if (user is null)
        {
            var firstName = update.Message?.From?.FirstName ?? update.CallbackQuery?.From?.FirstName ?? "Learner";
            user = User.Create(telegramUserId, null, firstName);
            db.Users.Add(user);
            db.Streaks.Add(Streak.Create(user.Id));
            await db.SaveChangesAsync(ct);
        }

        // Load or create bot session
        var session = await db.BotSessions
            .FirstOrDefaultAsync(s => s.UserId == user.Id && s.ChatId == chatId.Value, ct);
        if (session is null)
        {
            session = BotSession.Create(user.Id, chatId.Value);
            db.BotSessions.Add(session);
            await db.SaveChangesAsync(ct);
        }

        var ctx = new BotHandlerContext(update, user, session, bot, ct);

        try
        {
            await DispatchAsync(ctx);
            await db.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Bot handler error for user {UserId} state {State}", user.Id, session.State);
            await bot.SendMessage(chatId.Value, "Something went wrong. Please try again.", cancellationToken: ct);
        }
    }

    private Task DispatchAsync(BotHandlerContext ctx)
    {
        // /cancel is universal
        if (ctx.Update.Message?.Text == "/cancel")
        {
            ctx.Session.Transition("idle");
            return bot.SendMessage(ctx.ChatId, "Action cancelled.", cancellationToken: ctx.Ct);
        }

        return ctx.Session.State switch
        {
            "idle" => idleHandler.HandleAsync(ctx),
            "awaiting_image" => awaitingImageHandler.HandleAsync(ctx),
            "processing_exercise" => Task.CompletedTask, // passive — driven by Redis events
            "solving_exercise" => solvingExerciseHandler.HandleAsync(ctx),
            "showing_result" => showingResultHandler.HandleAsync(ctx),
            _ => idleHandler.HandleAsync(ctx)
        };
    }

    public async Task HandleErrorAsync(Exception exception, CancellationToken ct)
    {
        logger.LogError(exception, "Telegram polling error");
        await Task.CompletedTask;
    }
}
