using System.Text.Json;
using GrammarAi.Application.Features.Exercises.Commands;
using MediatR;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace GrammarAi.Infrastructure.Bot.Handlers;

public class ShowingResultHandler(IMediator mediator)
{
    private record ResultContext(Guid ExerciseId, List<AnswerEntry> Answers);
    private record AnswerEntry(Guid QuestionId, Guid? AnswerId, string? FreeText);

    public ShowingResultHandler(Application.Common.Interfaces.IAppDbContext db) : this((IMediator)null!) { }

    // Constructor used by SolvingExerciseHandler directly (without mediator)
    private readonly IMediator? _mediator = mediator;

    public async Task HandleAsync(BotHandlerContext ctx)
    {
        var resultCtx = JsonSerializer.Deserialize<ResultContext>(ctx.Session.Context)
            ?? throw new InvalidOperationException("Invalid result context.");

        if (_mediator is not null)
        {
            var submissions = resultCtx.Answers.Select(a =>
                new AnswerSubmission(a.QuestionId, a.AnswerId, a.FreeText, null)).ToList();

            var result = await _mediator.Send(
                new SolveExerciseCommand(ctx.User.Id, resultCtx.ExerciseId, submissions, "telegram"), ctx.Ct);

            var emoji = result.AccuracyPercent >= 80 ? "🌟" : result.AccuracyPercent >= 50 ? "👍" : "💪";
            var message = $"{emoji} *Exercise Complete!*\n\n" +
                          $"Score: {result.Score}/{result.MaxScore}\n" +
                          $"Accuracy: {result.AccuracyPercent}%\n\n";

            foreach (var item in result.Breakdown)
            {
                var icon = item.IsCorrect == true ? "✅" : item.IsCorrect == false ? "❌" : "❓";
                message += $"{icon} Q{result.Breakdown.IndexOf(item) + 1}\n";
                if (item.Explanation is not null)
                    message += $"   _{item.Explanation}_\n";
            }

            var buttons = new InlineKeyboardMarkup(new[]
            {
                new[] { InlineKeyboardButton.WithCallbackData("🔄 Try Again", $"retry:{resultCtx.ExerciseId}") },
                new[] { InlineKeyboardButton.WithCallbackData("📚 My Exercises", "cmd:myexercises") }
            });

            ctx.Session.Transition("idle");
            await ctx.Bot.SendMessage(ctx.ChatId, message,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                replyMarkup: buttons,
                cancellationToken: ctx.Ct);
        }
    }
}
