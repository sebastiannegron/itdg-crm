namespace Itdg.Crm.Api.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.FileName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(d => d.GoogleDriveFileId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(d => d.MimeType)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(d => d.CurrentVersion)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(d => d.FileSize)
            .IsRequired();

        builder.HasOne(d => d.Category)
            .WithMany()
            .HasForeignKey(d => d.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(d => new { d.TenantId, d.ClientId, d.CategoryId })
            .HasDatabaseName("IX_Document_TenantId_ClientId_CategoryId");
    }
}
