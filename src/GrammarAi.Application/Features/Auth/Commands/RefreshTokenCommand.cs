using System.Security.Cryptography;
using System.Text;
using GrammarAi.Application.Common.DTOs;
using GrammarAi.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace GrammarAi.Application.Features.Auth.Commands;

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResultDto>;

public class RefreshTokenCommandHandler(
    IAppDbContext db,
    IJwtService jwtService,
    IConfiguration config) : IRequestHandler<RefreshTokenCommand, AuthResultDto>
{
    public async Task<AuthResultDto> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(request.RefreshToken))).ToLower();

        var token = await db.AuthTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == hash, ct)
            ?? throw new UnauthorizedAccessException("Invalid refresh token.");

        if (!token.IsValid) throw new UnauthorizedAccessException("Token expired or revoked.");

        token.Revoke();

        var (rawNew, hashNew) = jwtService.GenerateRefreshToken();
        var expiry = DateTimeOffset.UtcNow.AddDays(int.Parse(config["Jwt:RefreshTokenExpiryDays"] ?? "30"));
        var newToken = Domain.Entities.AuthToken.Create(token.UserId, hashNew, expiry);
        db.AuthTokens.Add(newToken);

        await db.SaveChangesAsync(ct);

        var accessToken = jwtService.GenerateAccessToken(token.User.Id, token.User.DisplayName);
        var userDto = new UserDto(token.User.Id, token.User.DisplayName, token.User.AvatarUrl, token.User.TelegramId, token.User.WebEmail, token.User.IsPremium, token.User.CreatedAt);
        return new AuthResultDto(accessToken, rawNew, userDto);
    }
}
