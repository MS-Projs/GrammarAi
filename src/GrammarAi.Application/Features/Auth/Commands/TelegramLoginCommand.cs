using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using GrammarAi.Application.Common.DTOs;
using GrammarAi.Application.Common.Interfaces;
using GrammarAi.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace GrammarAi.Application.Features.Auth.Commands;

public record TelegramLoginCommand(string InitData) : IRequest<AuthResultDto>;

public class TelegramLoginCommandHandler(
    IAppDbContext db,
    IJwtService jwtService,
    IConfiguration config) : IRequestHandler<TelegramLoginCommand, AuthResultDto>
{
    public async Task<AuthResultDto> Handle(TelegramLoginCommand request, CancellationToken ct)
    {
        var parsed = ParseInitData(request.InitData);
        ValidateHmac(request.InitData, config["TelegramBot:Token"]!);

        var telegramId = long.Parse(parsed["id"]);
        var displayName = parsed.TryGetValue("first_name", out var fn) ? fn : "Learner";

        var user = await db.Users.FirstOrDefaultAsync(u => u.TelegramId == telegramId, ct);
        if (user is null)
        {
            user = User.Create(telegramId, null, displayName);
            db.Users.Add(user);

            var streak = Streak.Create(user.Id);
            db.Streaks.Add(streak);
        }

        var (rawToken, tokenHash) = jwtService.GenerateRefreshToken();
        var refreshTokenExpiry = DateTimeOffset.UtcNow.AddDays(
            int.Parse(config["Jwt:RefreshTokenExpiryDays"] ?? "30"));

        var authToken = AuthToken.Create(user.Id, tokenHash, refreshTokenExpiry);
        db.AuthTokens.Add(authToken);

        await db.SaveChangesAsync(ct);

        var accessToken = jwtService.GenerateAccessToken(user.Id, user.DisplayName);
        var userDto = new UserDto(user.Id, user.DisplayName, user.AvatarUrl, user.TelegramId, user.WebEmail, user.IsPremium, user.CreatedAt);

        return new AuthResultDto(accessToken, rawToken, userDto);
    }

    private static Dictionary<string, string> ParseInitData(string initData)
    {
        var result = new Dictionary<string, string>();
        foreach (var part in initData.Split('&'))
        {
            var idx = part.IndexOf('=');
            if (idx < 0) continue;
            var key = Uri.UnescapeDataString(part[..idx]);
            var val = Uri.UnescapeDataString(part[(idx + 1)..]);
            if (key == "user")
            {
                var userDoc = JsonDocument.Parse(val);
                foreach (var prop in userDoc.RootElement.EnumerateObject())
                    result[prop.Name] = prop.Value.ToString();
            }
            else
            {
                result[key] = val;
            }
        }
        return result;
    }

    private static void ValidateHmac(string initData, string botToken)
    {
        var pairs = initData.Split('&')
            .Where(p => !p.StartsWith("hash="))
            .OrderBy(p => p)
            .ToList();

        var hashEntry = initData.Split('&').FirstOrDefault(p => p.StartsWith("hash="));
        if (hashEntry is null) throw new UnauthorizedAccessException("Missing hash.");
        var receivedHash = hashEntry["hash=".Length..];

        var dataCheckString = string.Join("\n", pairs);
        using var hmacSecret = new HMACSHA256(Encoding.UTF8.GetBytes("WebAppData"));
        var secretKey = hmacSecret.ComputeHash(Encoding.UTF8.GetBytes(botToken));

        using var hmac = new HMACSHA256(secretKey);
        var computedHash = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(dataCheckString))).ToLower();

        if (!CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(computedHash),
                Encoding.UTF8.GetBytes(receivedHash)))
            throw new UnauthorizedAccessException("Invalid Telegram initData.");
    }
}
