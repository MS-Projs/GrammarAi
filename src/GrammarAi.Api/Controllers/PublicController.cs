using GrammarAi.Application.Features.Exercises.Queries;
using GrammarAi.Application.Features.Exercises.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GrammarAi.Api.Controllers;

[ApiController]
[Route("api/v1/public")]
public class PublicController(IMediator mediator) : ControllerBase
{
    [HttpGet("exercises")]
    public async Task<IActionResult> ListPublic(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] string? difficulty = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        // Public exercises are queried with a system user guid (empty) — the query handler checks is_public
        var result = await mediator.Send(
            new GetPublicExercisesQuery(page, Math.Clamp(limit, 1, 100), difficulty, search), ct);
        return Ok(result);
    }

    [HttpGet("exercises/{id:guid}")]
    public async Task<IActionResult> GetPublic(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetExerciseDetailQuery(id, null), ct);
        return result is null ? NotFound() : Ok(result);
    }
}
