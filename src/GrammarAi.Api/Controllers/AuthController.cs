using GrammarAi.Application.Features.Auth.Commands;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GrammarAi.Api.Controllers;

public class AuthController(IMediator mediator) : BaseApiController
{
    [HttpPost("telegram")]
    public async Task<IActionResult> TelegramLogin([FromBody] TelegramLoginRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new TelegramLoginCommand(req.InitData), ct);
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest req, CancellationToken ct)
    {
        var result = await mediator.Send(new RefreshTokenCommand(req.RefreshToken), ct);
        return Ok(result);
    }

    [HttpPost("logout")]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public IActionResult Logout()
    {
        // Client-side token disposal; refresh token revocation handled separately
        return NoContent();
    }
}

public record TelegramLoginRequest(string InitData);
public record RefreshTokenRequest(string RefreshToken);
