using GrammarAi.Application.Common.Interfaces;
using GrammarAi.Domain.Entities;
using GrammarAi.Domain.Enums;
using MediatR;

namespace GrammarAi.Application.Features.Exercises.Commands;

public record CreateExerciseCommand(
    Guid OwnerId,
    string? Title,
    string? Description,
    string? ExerciseType,
    string? Difficulty,
    List<string>? Tags,
    bool IsPublic
) : IRequest<Guid>;

public class CreateExerciseCommandHandler(IAppDbContext db) : IRequestHandler<CreateExerciseCommand, Guid>
{
    public async Task<Guid> Handle(CreateExerciseCommand req, CancellationToken ct)
    {
        var type = Enum.TryParse<ExerciseType>(req.ExerciseType, true, out var t) ? t : ExerciseType.MultipleChoice;
        var difficulty = Enum.TryParse<DifficultyLevel>(req.Difficulty, true, out var d) ? (DifficultyLevel?)d : null;

        var exercise = Exercise.Create(req.OwnerId, req.Title, type, difficulty, req.Tags, req.IsPublic);
        if (req.Description is not null) exercise.Description = req.Description;

        db.Exercises.Add(exercise);
        await db.SaveChangesAsync(ct);
        return exercise.Id;
    }
}
