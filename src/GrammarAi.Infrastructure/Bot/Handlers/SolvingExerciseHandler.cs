using System.Text.Json;
using GrammarAi.Application.Common.Interfaces;
using GrammarAi.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace GrammarAi.Infrastructure.Bot.Handlers;

public class SolvingExerciseHandler(IAppDbContext db)
{
    private record SolveContext(Guid ExerciseId, int QuestionIndex, List<AnswerEntry> Answers);
    private record AnswerEntry(Guid QuestionId, Guid? AnswerId, string? FreeText);

    public async Task HandleAsync(BotHandlerContext ctx)
    {
        var solveCtx = JsonSerializer.Deserialize<SolveContext>(ctx.Session.Context)
            ?? throw new InvalidOperationException("Invalid solve context.");

        var questions = await db.Questions
            .Include(q => q.Answers.OrderBy(a => a.OrderIndex))
            .Where(q => q.ExerciseId == solveCtx.ExerciseId)
            .OrderBy(q => q.OrderIndex)
            .ToListAsync(ctx.Ct);

        // Record answer from previous interaction
        if (ctx.CallbackData?.StartsWith("ans:") == true || ctx.MessageText is not null)
        {
            var currentQ = questions[solveCtx.QuestionIndex];
            Guid? answerId = null;
            string? freeText = null;

            if (ctx.CallbackData?.StartsWith("ans:") == true)
                answerId = Guid.Parse(ctx.CallbackData["ans:".Length..]);
            else
                freeText = ctx.MessageText;

            var answers = solveCtx.Answers;
            answers.Add(new AnswerEntry(currentQ.Id, answerId, freeText));

            var nextIndex = solveCtx.QuestionIndex + 1;

            if (nextIndex >= questions.Count)
            {
                // Move to result state
                ctx.Session.Transition("showing_result",
                    JsonSerializer.Serialize(new { solveCtx.ExerciseId, Answers = answers }));
                // Trigger result display
                await new ShowingResultHandler(db).HandleAsync(ctx);
                return;
            }

            // Advance question
            ctx.Session.Transition("solving_exercise",
                JsonSerializer.Serialize(new SolveContext(solveCtx.ExerciseId, nextIndex, answers)));
            await RenderQuestionAsync(ctx, questions[nextIndex], nextIndex, questions.Count);
        }
        else
        {
            // First render
            await RenderQuestionAsync(ctx, questions[solveCtx.QuestionIndex], solveCtx.QuestionIndex, questions.Count);
        }
    }

    private static async Task RenderQuestionAsync(BotHandlerContext ctx, Domain.Entities.Question question, int index, int total)
    {
        var header = $"*Question {index + 1}/{total}*\n\n{EscapeMarkdown(question.Body)}";

        if (question.ExerciseType == ExerciseType.MultipleChoice || question.ExerciseType == ExerciseType.TrueFalse)
        {
            var buttons = question.Answers.Select(a =>
                new[] { InlineKeyboardButton.WithCallbackData(a.Text, $"ans:{a.Id}") }).ToList();

            await ctx.Bot.SendMessage(ctx.ChatId, header,
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                replyMarkup: new InlineKeyboardMarkup(buttons),
                cancellationToken: ctx.Ct);
        }
        else
        {
            await ctx.Bot.SendMessage(ctx.ChatId,
                header + "\n\n_Type your answer:_",
                parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown,
                cancellationToken: ctx.Ct);
        }
    }

    private static string EscapeMarkdown(string text) =>
        text.Replace("_", "\\_").Replace("*", "\\*").Replace("[", "\\[").Replace("`", "\\`");
}
