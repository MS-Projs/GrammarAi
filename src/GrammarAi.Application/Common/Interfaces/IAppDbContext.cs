using GrammarAi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GrammarAi.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<User> Users { get; }
    DbSet<AuthToken> AuthTokens { get; }
    DbSet<BotSession> BotSessions { get; }
    DbSet<Exercise> Exercises { get; }
    DbSet<ExerciseImage> ExerciseImages { get; }
    DbSet<Question> Questions { get; }
    DbSet<Answer> Answers { get; }
    DbSet<UserAnswer> UserAnswers { get; }
    DbSet<Streak> Streaks { get; }
    DbSet<OcrJob> OcrJobs { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
