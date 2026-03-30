using GrammarAi.Application.Common.Interfaces;
using GrammarAi.Application.Features.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GrammarAi.Api.Controllers;

[Authorize]
public class UsersController(IMediator mediator, IAppDbContext db) : BaseApiController
{
    [HttpGet("me")]
    public async Task<IActionResult> GetMe(CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == CurrentUserId, ct);
        if (user is null) return NotFound();
        return Ok(new
        {
            user.Id, user.DisplayName, user.AvatarUrl,
            user.TelegramId, user.WebEmail, user.IsPremium, user.CreatedAt
        });
    }

    [HttpPatch("me")]
    public async Task<IActionResult> UpdateMe([FromBody] UpdateMeRequest req, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == CurrentUserId, ct);
        if (user is null) return NotFound();
        user.UpdateProfile(req.DisplayName ?? user.DisplayName, req.Timezone, req.LanguageCode);
        await db.SaveChangesAsync(ct);
        return Ok(new { user.Id, user.DisplayName, user.Timezone, user.LanguageCode });
    }

    [HttpGet("me/stats")]
    public async Task<IActionResult> GetStats(CancellationToken ct)
    {
        var result = await mediator.Send(new GetMyStatsQuery(CurrentUserId), ct);
        return Ok(result);
    }

    [HttpGet("me/streak")]
    public async Task<IActionResult> GetStreak(CancellationToken ct)
    {
        var streak = await db.Streaks.FirstOrDefaultAsync(s => s.UserId == CurrentUserId, ct);
        return Ok(new
        {
            CurrentStreak = streak?.CurrentStreak ?? 0,
            LongestStreak = streak?.LongestStreak ?? 0,
            LastActiveDate = streak?.LastActiveDate
        });
    }
}

public record UpdateMeRequest(string? DisplayName, string? Timezone, string? LanguageCode);
