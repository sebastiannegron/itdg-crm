namespace Itdg.Crm.Api.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class EmailMirrorConfiguration : IEntityTypeConfiguration<EmailMirror>
{
    public void Configure(EntityTypeBuilder<EmailMirror> builder)
    {
        builder.ToTable("EmailMirrors");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.GmailMessageId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.GmailThreadId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.Subject)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.From)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.To)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(e => e.BodyPreview)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.HasAttachments)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(e => new { e.TenantId, e.ClientId, e.ReceivedAt })
            .HasDatabaseName("IX_EmailMirror_TenantId_ClientId_ReceivedAt");

        builder.HasIndex(e => e.GmailMessageId)
            .IsUnique()
            .HasDatabaseName("IX_EmailMirror_GmailMessageId");
    }
}
