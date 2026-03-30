using GrammarAi.Application.Common.DTOs;
using GrammarAi.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GrammarAi.Application.Features.Users.Queries;

public record GetMyStatsQuery(Guid UserId) : IRequest<UserStatsDto>;

public class GetMyStatsQueryHandler(IAppDbContext db) : IRequestHandler<GetMyStatsQuery, UserStatsDto>
{
    public async Task<UserStatsDto> Handle(GetMyStatsQuery req, CancellationToken ct)
    {
        var totalExercises = await db.Exercises.CountAsync(e => e.OwnerId == req.UserId, ct);
        var totalQuestions = await db.UserAnswers.CountAsync(a => a.UserId == req.UserId, ct);
        var correctAnswers = await db.UserAnswers.CountAsync(a => a.UserId == req.UserId && a.IsCorrect == true, ct);
        var accuracy = totalQuestions > 0
            ? Math.Round((decimal)correctAnswers / totalQuestions * 100, 1) : 0;

        var streak = await db.Streaks.FirstOrDefaultAsync(s => s.UserId == req.UserId, ct);

        return new UserStatsDto(
            totalExercises, totalQuestions, correctAnswers, accuracy,
            streak?.CurrentStreak ?? 0, streak?.LongestStreak ?? 0);
    }
}
