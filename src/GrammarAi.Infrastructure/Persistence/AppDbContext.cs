using GrammarAi.Application.Common.Interfaces;
using GrammarAi.Domain.Entities;
using GrammarAi.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GrammarAi.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options), IAppDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<AuthToken> AuthTokens => Set<AuthToken>();
    public DbSet<BotSession> BotSessions => Set<BotSession>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<ExerciseImage> ExerciseImages => Set<ExerciseImage>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<Answer> Answers => Set<Answer>();
    public DbSet<UserAnswer> UserAnswers => Set<UserAnswer>();
    public DbSet<Streak> Streaks => Set<Streak>();
    public DbSet<OcrJob> OcrJobs => Set<OcrJob>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        base.OnModelCreating(model);
        model.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
