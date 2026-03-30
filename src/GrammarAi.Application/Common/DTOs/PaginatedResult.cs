namespace GrammarAi.Application.Common.DTOs;

public record PaginatedResult<T>(List<T> Data, int Page, int Limit, int Total)
{
    public int TotalPages => (int)Math.Ceiling((double)Total / Limit);
}
