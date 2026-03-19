namespace Itdg.Crm.Api.Infrastructure.Data.Configurations;

using Itdg.Crm.Api.Domain.GeneralConstants;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
{
    public void Configure(EntityTypeBuilder<NotificationPreference> builder)
    {
        builder.ToTable("NotificationPreferences");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.EventType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.Channel)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.DigestMode)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(p => new { p.UserId, p.EventType, p.Channel })
            .IsUnique()
            .HasDatabaseName("IX_NotificationPreference_UserId_EventType_Channel");
    }
}
