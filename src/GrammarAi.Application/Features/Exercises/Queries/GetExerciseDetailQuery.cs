using GrammarAi.Application.Common.DTOs;
using GrammarAi.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GrammarAi.Application.Features.Exercises.Queries;

public record GetExerciseDetailQuery(Guid ExerciseId, Guid? UserId) : IRequest<ExerciseDetailDto?>;

public class GetExerciseDetailQueryHandler(IAppDbContext db) : IRequestHandler<GetExerciseDetailQuery, ExerciseDetailDto?>
{
    public async Task<ExerciseDetailDto?> Handle(GetExerciseDetailQuery req, CancellationToken ct)
    {
        var exercise = await db.Exercises
            .Include(e => e.Questions.OrderBy(q => q.OrderIndex))
                .ThenInclude(q => q.Answers.OrderBy(a => a.OrderIndex))
            .FirstOrDefaultAsync(e => e.Id == req.ExerciseId
                && (e.OwnerId == req.UserId || e.IsPublic), ct);

        if (exercise is null) return null;

        var questions = exercise.Questions.Select(q => new QuestionDto(
            q.Id, q.OrderIndex, q.Body, q.Explanation,
            q.ExerciseType.ToString(), q.MaxScore,
            q.Answers.Select(a => new AnswerDto(a.Id, a.OrderIndex, a.Text, a.IsCorrect, a.Explanation)).ToList()
        )).ToList();

        return new ExerciseDetailDto(
            exercise.Id, exercise.Title, exercise.Description,
            exercise.ExerciseType.ToString(), exercise.Difficulty?.ToString(),
            exercise.Status.ToString(), exercise.IsPublic, exercise.Tags,
            questions, exercise.CreatedAt);
    }
}
