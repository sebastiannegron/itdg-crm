namespace Itdg.Crm.Api.Infrastructure.Data.Configurations;

using Itdg.Crm.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Subject)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(m => m.Body)
            .IsRequired();

        builder.Property(m => m.Direction)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(m => m.IsPortalMessage)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(m => m.IsRead)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(m => m.Attachments)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(m => new { m.TenantId, m.ClientId })
            .HasDatabaseName("IX_Message_TenantId_ClientId");

        builder.HasIndex(m => new { m.TenantId, m.ClientId, m.IsRead })
            .HasDatabaseName("IX_Message_TenantId_ClientId_IsRead");
    }
}
