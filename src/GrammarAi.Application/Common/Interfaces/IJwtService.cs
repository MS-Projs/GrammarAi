namespace GrammarAi.Application.Common.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(Guid userId, string displayName);
    (string Raw, string Hash) GenerateRefreshToken();
    Guid? ValidateAccessToken(string token);
}
