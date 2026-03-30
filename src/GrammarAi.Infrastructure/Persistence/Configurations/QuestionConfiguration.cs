using GrammarAi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GrammarAi.Infrastructure.Persistence.Configurations;

public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> b)
    {
        b.ToTable("questions");
        b.HasKey(q => q.Id);
        b.Property(q => q.Id).HasColumnName("id");
        b.Property(q => q.ExerciseId).HasColumnName("exercise_id");
        b.Property(q => q.OrderIndex).HasColumnName("order_index");
        b.Property(q => q.Body).HasColumnName("body").IsRequired();
        b.Property(q => q.Explanation).HasColumnName("explanation");
        b.Property(q => q.ExerciseType).HasColumnName("exercise_type").HasConversion<string>().HasMaxLength(32);
        b.Property(q => q.MaxScore).HasColumnName("max_score").HasDefaultValue(1);
        b.Property(q => q.Metadata).HasColumnName("metadata").HasColumnType("jsonb").HasDefaultValue("{}");

        b.HasMany(q => q.Answers).WithOne(a => a.Question).HasForeignKey(a => a.QuestionId).OnDelete(DeleteBehavior.Cascade);
        b.HasMany(q => q.UserAnswers).WithOne(ua => ua.Question).HasForeignKey(ua => ua.QuestionId).OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(q => q.ExerciseId);
    }
}
