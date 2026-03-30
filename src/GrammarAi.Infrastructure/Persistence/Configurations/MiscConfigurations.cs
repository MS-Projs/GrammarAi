using GrammarAi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GrammarAi.Infrastructure.Persistence.Configurations;

public class AuthTokenConfiguration : IEntityTypeConfiguration<AuthToken>
{
    public void Configure(EntityTypeBuilder<AuthToken> b)
    {
        b.ToTable("auth_tokens");
        b.HasKey(t => t.Id);
        b.Property(t => t.UserId).HasColumnName("user_id");
        b.Property(t => t.TokenHash).HasColumnName("token_hash").HasMaxLength(128).IsRequired();
        b.Property(t => t.ExpiresAt).HasColumnName("expires_at");
        b.Property(t => t.CreatedAt).HasColumnName("created_at");
        b.Property(t => t.RevokedAt).HasColumnName("revoked_at");
        b.HasIndex(t => t.TokenHash).IsUnique();
        b.HasIndex(t => t.UserId);
    }
}

public class BotSessionConfiguration : IEntityTypeConfiguration<BotSession>
{
    public void Configure(EntityTypeBuilder<BotSession> b)
    {
        b.ToTable("bot_sessions");
        b.HasKey(s => s.Id);
        b.Property(s => s.UserId).HasColumnName("user_id");
        b.Property(s => s.ChatId).HasColumnName("chat_id");
        b.Property(s => s.State).HasColumnName("state").HasMaxLength(64).HasDefaultValue("idle");
        b.Property(s => s.Context).HasColumnName("context").HasColumnType("jsonb").HasDefaultValue("{}");
        b.Property(s => s.LastMessageId).HasColumnName("last_message_id");
        b.Property(s => s.UpdatedAt).HasColumnName("updated_at");
        b.HasIndex(s => new { s.UserId, s.ChatId }).IsUnique();
    }
}

public class ExerciseImageConfiguration : IEntityTypeConfiguration<ExerciseImage>
{
    public void Configure(EntityTypeBuilder<ExerciseImage> b)
    {
        b.ToTable("exercise_images");
        b.HasKey(i => i.Id);
        b.Property(i => i.ExerciseId).HasColumnName("exercise_id");
        b.Property(i => i.FileKey).HasColumnName("file_key").IsRequired();
        b.Property(i => i.FileUrl).HasColumnName("file_url").IsRequired();
        b.Property(i => i.TelegramFileId).HasColumnName("telegram_file_id");
        b.Property(i => i.MimeType).HasColumnName("mime_type").HasMaxLength(64);
        b.Property(i => i.SizeBytes).HasColumnName("size_bytes");
        b.Property(i => i.PageNumber).HasColumnName("page_number").HasDefaultValue(1);
        b.Property(i => i.OcrRaw).HasColumnName("ocr_raw");
        b.Property(i => i.UploadedAt).HasColumnName("uploaded_at");
        b.HasIndex(i => i.ExerciseId);
    }
}

public class AnswerConfiguration : IEntityTypeConfiguration<Answer>
{
    public void Configure(EntityTypeBuilder<Answer> b)
    {
        b.ToTable("answers");
        b.HasKey(a => a.Id);
        b.Property(a => a.QuestionId).HasColumnName("question_id");
        b.Property(a => a.OrderIndex).HasColumnName("order_index");
        b.Property(a => a.Text).HasColumnName("text").IsRequired();
        b.Property(a => a.IsCorrect).HasColumnName("is_correct").HasDefaultValue(false);
        b.Property(a => a.Explanation).HasColumnName("explanation");
        b.HasIndex(a => a.QuestionId);
    }
}

public class UserAnswerConfiguration : IEntityTypeConfiguration<UserAnswer>
{
    public void Configure(EntityTypeBuilder<UserAnswer> b)
    {
        b.ToTable("user_answers");
        b.HasKey(ua => ua.Id);
        b.Property(ua => ua.UserId).HasColumnName("user_id");
        b.Property(ua => ua.QuestionId).HasColumnName("question_id");
        b.Property(ua => ua.AnswerId).HasColumnName("answer_id");
        b.Property(ua => ua.FreeText).HasColumnName("free_text");
        b.Property(ua => ua.IsCorrect).HasColumnName("is_correct");
        b.Property(ua => ua.Score).HasColumnName("score").HasPrecision(4, 2);
        b.Property(ua => ua.TimeSpentMs).HasColumnName("time_spent_ms");
        b.Property(ua => ua.Platform).HasColumnName("platform").HasMaxLength(16);
        b.Property(ua => ua.AnsweredAt).HasColumnName("answered_at");
        b.HasOne(ua => ua.Answer).WithMany().HasForeignKey(ua => ua.AnswerId).OnDelete(DeleteBehavior.SetNull);
        b.HasIndex(ua => ua.UserId);
        b.HasIndex(ua => ua.QuestionId);
    }
}

public class StreakConfiguration : IEntityTypeConfiguration<Streak>
{
    public void Configure(EntityTypeBuilder<Streak> b)
    {
        b.ToTable("streaks");
        b.HasKey(s => s.UserId);
        b.Property(s => s.UserId).HasColumnName("user_id");
        b.Property(s => s.CurrentStreak).HasColumnName("current_streak").HasDefaultValue(0);
        b.Property(s => s.LongestStreak).HasColumnName("longest_streak").HasDefaultValue(0);
        b.Property(s => s.LastActiveDate).HasColumnName("last_active_date");
        b.Property(s => s.UpdatedAt).HasColumnName("updated_at");
        b.HasOne(s => s.User).WithOne(u => u.Streak).HasForeignKey<Streak>(s => s.UserId);
    }
}

public class OcrJobConfiguration : IEntityTypeConfiguration<OcrJob>
{
    public void Configure(EntityTypeBuilder<OcrJob> b)
    {
        b.ToTable("ocr_jobs");
        b.HasKey(j => j.Id);
        b.Property(j => j.ExerciseId).HasColumnName("exercise_id");
        b.Property(j => j.HangfireJobId).HasColumnName("hangfire_job_id");
        b.Property(j => j.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(16);
        b.Property(j => j.AttemptCount).HasColumnName("attempt_count").HasDefaultValue(0);
        b.Property(j => j.ErrorDetails).HasColumnName("error_details");
        b.Property(j => j.StartedAt).HasColumnName("started_at");
        b.Property(j => j.FinishedAt).HasColumnName("finished_at");
        b.Property(j => j.CreatedAt).HasColumnName("created_at");
        b.HasIndex(j => j.ExerciseId);
        b.HasIndex(j => j.Status);
    }
}
