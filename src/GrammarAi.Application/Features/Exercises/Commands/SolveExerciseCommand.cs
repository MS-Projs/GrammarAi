using GrammarAi.Application.Common.DTOs;
using GrammarAi.Application.Common.Interfaces;
using GrammarAi.Domain.Entities;
using GrammarAi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GrammarAi.Application.Features.Exercises.Commands;

public record AnswerSubmission(Guid QuestionId, Guid? AnswerId, string? FreeText, int? TimeSpentMs);

public record SolveExerciseCommand(
    Guid UserId,
    Guid ExerciseId,
    List<AnswerSubmission> Answers,
    string Platform = "web"
) : IRequest<SolveResultDto>;

public class SolveExerciseCommandHandler(
    IAppDbContext db,
    IAiService ai) : IRequestHandler<SolveExerciseCommand, SolveResultDto>
{
    public async Task<SolveResultDto> Handle(SolveExerciseCommand req, CancellationToken ct)
    {
        var questions = await db.Questions
            .Include(q => q.Answers)
            .Where(q => q.ExerciseId == req.ExerciseId)
            .ToListAsync(ct);

        if (questions.Count == 0)
            throw new KeyNotFoundException("Exercise not found or has no questions.");

        var breakdown = new List<AnswerBreakdownDto>();
        decimal totalScore = 0;
        decimal maxScore = questions.Sum(q => q.MaxScore);

        foreach (var submission in req.Answers)
        {
            var question = questions.FirstOrDefault(q => q.Id == submission.QuestionId);
            if (question is null) continue;

            bool? isCorrect = null;
            decimal score = 0;
            var correctIds = question.Answers.Where(a => a.IsCorrect).Select(a => a.Id).ToList();

            if (question.ExerciseType == ExerciseType.Essay)
            {
                if (!string.IsNullOrWhiteSpace(submission.FreeText))
                    isCorrect = await ai.EvaluateEssayAnswerAsync(question.Body, submission.FreeText, ct);
            }
            else if (question.ExerciseType == ExerciseType.FillBlank)
            {
                // Case-insensitive match against any correct answer text
                isCorrect = correctIds.Count > 0
                    ? question.Answers.Any(a => a.IsCorrect &&
                        string.Equals(a.Text.Trim(), submission.FreeText?.Trim(), StringComparison.OrdinalIgnoreCase))
                    : !string.IsNullOrWhiteSpace(submission.FreeText) &&
                      await ai.EvaluateEssayAnswerAsync(question.Body, submission.FreeText, ct);
            }
            else if (submission.AnswerId.HasValue)
            {
                var chosen = question.Answers.FirstOrDefault(a => a.Id == submission.AnswerId.Value);
                isCorrect = chosen?.IsCorrect ?? false;
            }

            if (isCorrect == true) score = question.MaxScore;
            totalScore += score;

            var explanation = question.Answers.FirstOrDefault(a => a.IsCorrect)?.Explanation ?? question.Explanation;
            breakdown.Add(new AnswerBreakdownDto(question.Id, isCorrect, score, correctIds, explanation));

            var userAnswer = UserAnswer.Create(req.UserId, question.Id, submission.AnswerId,
                submission.FreeText, req.Platform, submission.TimeSpentMs);
            userAnswer.IsCorrect = isCorrect;
            userAnswer.Score = score;
            db.UserAnswers.Add(userAnswer);
        }

        // Update streak
        var streak = await db.Streaks.FirstOrDefaultAsync(s => s.UserId == req.UserId, ct);
        if (streak is null)
        {
            streak = Streak.Create(req.UserId);
            db.Streaks.Add(streak);
        }
        streak.RecordActivity();

        await db.SaveChangesAsync(ct);

        var accuracy = maxScore > 0 ? Math.Round(totalScore / maxScore * 100, 1) : 0;
        return new SolveResultDto(totalScore, maxScore, accuracy, breakdown);
    }
}
