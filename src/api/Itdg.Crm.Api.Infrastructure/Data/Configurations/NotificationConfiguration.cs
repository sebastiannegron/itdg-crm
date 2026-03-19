namespace Itdg.Crm.Api.Infrastructure.Data.Configurations;

using Itdg.Crm.Api.Domain.GeneralConstants;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(n => n.Body)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(n => n.EventType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(n => n.Channel)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(n => n.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(n => new { n.UserId, n.Status })
            .HasDatabaseName("IX_Notification_UserId_Status");
    }
}
