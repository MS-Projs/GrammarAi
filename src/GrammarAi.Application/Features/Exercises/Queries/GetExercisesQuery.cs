using GrammarAi.Application.Common.DTOs;
using GrammarAi.Application.Common.Interfaces;
using GrammarAi.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GrammarAi.Application.Features.Exercises.Queries;

public record GetExercisesQuery(
    Guid UserId,
    int Page,
    int Limit,
    string? Status,
    string? Difficulty,
    List<string>? Tags,
    string? Search
) : IRequest<PaginatedResult<ExerciseDto>>;

public class GetExercisesQueryHandler(IAppDbContext db) : IRequestHandler<GetExercisesQuery, PaginatedResult<ExerciseDto>>
{
    public async Task<PaginatedResult<ExerciseDto>> Handle(GetExercisesQuery req, CancellationToken ct)
    {
        var query = db.Exercises.Where(e => e.OwnerId == req.UserId).AsQueryable();

        if (Enum.TryParse<ExerciseStatus>(req.Status, true, out var status))
            query = query.Where(e => e.Status == status);

        if (Enum.TryParse<DifficultyLevel>(req.Difficulty, true, out var diff))
            query = query.Where(e => e.Difficulty == diff);

        if (req.Tags is { Count: > 0 })
            query = query.Where(e => e.Tags.Any(t => req.Tags.Contains(t)));

        if (!string.IsNullOrWhiteSpace(req.Search))
            query = query.Where(e => EF.Functions.ILike(e.Title ?? "", $"%{req.Search}%"));

        var total = await query.CountAsync(ct);

        var data = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((req.Page - 1) * req.Limit)
            .Take(req.Limit)
            .Select(e => new ExerciseDto(
                e.Id, e.Title, e.Description,
                e.ExerciseType.ToString(), e.Difficulty.ToString(),
                e.Status.ToString(), e.IsPublic, e.Tags, e.CreatedAt))
            .ToListAsync(ct);

        return new PaginatedResult<ExerciseDto>(data, req.Page, req.Limit, total);
    }
}
