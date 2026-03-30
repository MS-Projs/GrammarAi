using GrammarAi.Infrastructure.Bot;
using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;

namespace GrammarAi.Api.Controllers;

[ApiController]
[Route("bot")]
public class BotController(BotUpdateHandler handler) : ControllerBase
{
    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook([FromBody] Update update, CancellationToken ct)
    {
        await handler.HandleUpdateAsync(update, ct);
        return Ok();
    }
}
