using GrammarAi.Application.Common.Interfaces;
using GrammarAi.Application.Features.Exercises.Commands;
using GrammarAi.Application.Features.Exercises.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GrammarAi.Api.Controllers;

[Authorize]
public class ExercisesController(IMediator mediator, IAppDbContext db) : BaseApiController
{
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] string? status = null,
        [FromQuery] string? difficulty = null,
        [FromQuery] string? tags = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var tagList = tags?.Split(',').Where(t => !string.IsNullOrWhiteSpace(t)).ToList();
        var result = await mediator.Send(
            new GetExercisesQuery(CurrentUserId, page, Math.Clamp(limit, 1, 100), status, difficulty, tagList, search), ct);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateExerciseRequest req, CancellationToken ct)
    {
        var id = await mediator.Send(
            new CreateExerciseCommand(CurrentUserId, req.Title, req.Description, req.ExerciseType, req.Difficulty, req.Tags, req.IsPublic ?? false), ct);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetExerciseDetailQuery(id, CurrentUserId), ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateExerciseRequest req, CancellationToken ct)
    {
        var exercise = await db.Exercises
            .FirstOrDefaultAsync(e => e.Id == id && e.OwnerId == CurrentUserId, ct);
        if (exercise is null) return NotFound();

        if (req.Title is not null) exercise.Title = req.Title;
        if (req.Description is not null) exercise.Description = req.Description;
        if (req.IsPublic.HasValue) exercise.IsPublic = req.IsPublic.Value;
        if (req.Tags is not null) exercise.Tags = req.Tags;
        exercise.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);
        return Ok(new { exercise.Id, exercise.Title, exercise.IsPublic, exercise.Tags });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var exercise = await db.Exercises
            .FirstOrDefaultAsync(e => e.Id == id && e.OwnerId == CurrentUserId, ct);
        if (exercise is null) return NotFound();
        db.Exercises.Remove(exercise);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    [HttpGet("{id:guid}/status")]
    public async Task<IActionResult> GetStatus(Guid id, CancellationToken ct)
    {
        var exercise = await db.Exercises
            .FirstOrDefaultAsync(e => e.Id == id && (e.OwnerId == CurrentUserId || e.IsPublic), ct);
        if (exercise is null) return NotFound();
        return Ok(new { exercise.Id, Status = exercise.Status.ToString(), exercise.ErrorMessage });
    }

    [HttpPost("{id:guid}/retry")]
    public async Task<IActionResult> Retry(Guid id,
        [FromServices] IBackgroundJobService jobs,
        CancellationToken ct)
    {
        var exercise = await db.Exercises
            .FirstOrDefaultAsync(e => e.Id == id && e.OwnerId == CurrentUserId, ct);
        if (exercise is null) return NotFound();
        if (exercise.Status != Domain.Enums.ExerciseStatus.Failed)
            return BadRequest(new { error = "Exercise is not in a failed state." });

        exercise.MarkProcessing();
        var hangfireId = jobs.EnqueueOcrJob(id);

        var job = await db.OcrJobs.FirstOrDefaultAsync(j => j.ExerciseId == id, ct)
            ?? Domain.Entities.OcrJob.Create(id);
        job.Start(hangfireId);
        if (!db.OcrJobs.Contains(job)) db.OcrJobs.Add(job);

        await db.SaveChangesAsync(ct);
        return Accepted(new { jobId = hangfireId });
    }

    [HttpPost("{id:guid}/images")]
    [RequestSizeLimit(52_428_800)] // 50 MB total
    public async Task<IActionResult> UploadImages(Guid id, IFormFileCollection files, CancellationToken ct)
    {
        if (files.Count == 0) return BadRequest(new { error = "No files provided." });
        if (files.Count > 5) return BadRequest(new { error = "Maximum 5 files per upload." });

        var uploads = files.Select(f =>
            new ImageUpload(f.OpenReadStream(), f.FileName, f.ContentType)).ToList();

        var results = await mediator.Send(
            new UploadExerciseImagesCommand(id, CurrentUserId, uploads), ct);

        return Ok(new { images = results });
    }

    [HttpPost("{id:guid}/solve")]
    public async Task<IActionResult> Solve(Guid id, [FromBody] SolveRequest req, CancellationToken ct)
    {
        var submissions = req.Answers.Select(a =>
            new AnswerSubmission(a.QuestionId, a.AnswerId, a.FreeText, a.TimeSpentMs)).ToList();

        var result = await mediator.Send(
            new SolveExerciseCommand(CurrentUserId, id, submissions, "web"), ct);
        return Ok(result);
    }

    [HttpGet("{id:guid}/results")]
    public async Task<IActionResult> GetResults(Guid id, CancellationToken ct)
    {
        var answers = await db.UserAnswers
            .Where(ua => ua.UserId == CurrentUserId
                && db.Questions.Any(q => q.Id == ua.QuestionId && q.ExerciseId == id))
            .OrderByDescending(ua => ua.AnsweredAt)
            .ToListAsync(ct);

        if (answers.Count == 0) return NotFound(new { error = "No attempts found." });

        var score = answers.Sum(a => a.Score);
        var max = answers.Count;
        var correct = answers.Count(a => a.IsCorrect == true);
        return Ok(new { score, maxScore = max, accuracyPercent = max > 0 ? Math.Round((double)correct / max * 100, 1) : 0, attemptedAt = answers[0].AnsweredAt });
    }
}

// Request models
public record CreateExerciseRequest(string? Title, string? Description, string? ExerciseType, string? Difficulty, List<string>? Tags, bool? IsPublic);
public record UpdateExerciseRequest(string? Title, string? Description, bool? IsPublic, List<string>? Tags);
public record SolveRequest(List<SolveAnswerItem> Answers);
public record SolveAnswerItem(Guid QuestionId, Guid? AnswerId, string? FreeText, int? TimeSpentMs);
