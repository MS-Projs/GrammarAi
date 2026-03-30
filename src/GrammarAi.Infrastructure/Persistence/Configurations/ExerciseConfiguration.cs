using GrammarAi.Domain.Entities;
using GrammarAi.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GrammarAi.Infrastructure.Persistence.Configurations;

public class ExerciseConfiguration : IEntityTypeConfiguration<Exercise>
{
    public void Configure(EntityTypeBuilder<Exercise> b)
    {
        b.ToTable("exercises");
        b.HasKey(e => e.Id);
        b.Property(e => e.Id).HasColumnName("id");
        b.Property(e => e.OwnerId).HasColumnName("owner_id");
        b.Property(e => e.Title).HasColumnName("title").HasMaxLength(256);
        b.Property(e => e.Description).HasColumnName("description");
        b.Property(e => e.SourceText).HasColumnName("source_text");
        b.Property(e => e.ExerciseType).HasColumnName("exercise_type")
            .HasConversion<string>().HasMaxLength(32);
        b.Property(e => e.Difficulty).HasColumnName("difficulty")
            .HasConversion<string>().HasMaxLength(4);
        b.Property(e => e.Status).HasColumnName("status")
            .HasConversion<string>().HasMaxLength(16);
        b.Property(e => e.ErrorMessage).HasColumnName("error_message");
        b.Property(e => e.IsPublic).HasColumnName("is_public").HasDefaultValue(false);
        b.Property(e => e.Tags).HasColumnName("tags")
            .HasColumnType("text[]");
        b.Property(e => e.Metadata).HasColumnName("metadata").HasColumnType("jsonb").HasDefaultValue("{}");
        b.Property(e => e.CreatedAt).HasColumnName("created_at");
        b.Property(e => e.UpdatedAt).HasColumnName("updated_at");

        b.HasOne(e => e.Owner).WithMany(u => u.Exercises).HasForeignKey(e => e.OwnerId).OnDelete(DeleteBehavior.SetNull);
        b.HasMany(e => e.Images).WithOne(i => i.Exercise).HasForeignKey(i => i.ExerciseId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(e => e.Questions).WithOne(q => q.Exercise).HasForeignKey(q => q.ExerciseId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(e => e.OcrJob).WithOne(j => j.Exercise).HasForeignKey<OcrJob>(j => j.ExerciseId);

        b.HasIndex(e => e.OwnerId);
        b.HasIndex(e => e.Status);
    }
}
