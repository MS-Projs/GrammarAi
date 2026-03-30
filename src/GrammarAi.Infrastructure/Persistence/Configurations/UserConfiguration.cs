using GrammarAi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GrammarAi.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("users");
        b.HasKey(u => u.Id);
        b.Property(u => u.Id).HasColumnName("id");
        b.Property(u => u.TelegramId).HasColumnName("telegram_id");
        b.Property(u => u.WebEmail).HasColumnName("web_email").HasMaxLength(256);
        b.Property(u => u.DisplayName).HasColumnName("display_name").HasMaxLength(128).IsRequired();
        b.Property(u => u.AvatarUrl).HasColumnName("avatar_url");
        b.Property(u => u.LanguageCode).HasColumnName("language_code").HasMaxLength(2).HasDefaultValue("en");
        b.Property(u => u.Timezone).HasColumnName("timezone").HasMaxLength(64).HasDefaultValue("UTC");
        b.Property(u => u.IsPremium).HasColumnName("is_premium").HasDefaultValue(false);
        b.Property(u => u.IsBlocked).HasColumnName("is_blocked").HasDefaultValue(false);
        b.Property(u => u.CreatedAt).HasColumnName("created_at");
        b.Property(u => u.UpdatedAt).HasColumnName("updated_at");

        b.HasIndex(u => u.TelegramId).IsUnique().HasFilter("telegram_id IS NOT NULL");
        b.HasIndex(u => u.WebEmail).IsUnique().HasFilter("web_email IS NOT NULL");
    }
}
