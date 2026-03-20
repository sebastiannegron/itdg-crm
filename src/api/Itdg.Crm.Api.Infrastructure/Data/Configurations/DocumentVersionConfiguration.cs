namespace Itdg.Crm.Api.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class DocumentVersionConfiguration : IEntityTypeConfiguration<DocumentVersion>
{
    public void Configure(EntityTypeBuilder<DocumentVersion> builder)
    {
        builder.ToTable("DocumentVersions");

        builder.HasKey(dv => dv.Id);

        builder.Property(dv => dv.VersionNumber)
            .IsRequired();

        builder.Property(dv => dv.GoogleDriveFileId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(dv => dv.UploadedAt)
            .IsRequired();

        builder.HasOne(dv => dv.Document)
            .WithMany()
            .HasForeignKey(dv => dv.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(dv => new { dv.DocumentId, dv.VersionNumber })
            .IsUnique()
            .HasDatabaseName("IX_DocumentVersion_DocumentId_VersionNumber");
    }
}
