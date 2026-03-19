namespace Itdg.Crm.Api.Infrastructure.Data.Configurations;

using Itdg.Crm.Api.Domain.Enums;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CommunicationTemplateConfiguration : IEntityTypeConfiguration<CommunicationTemplate>
{
    public void Configure(EntityTypeBuilder<CommunicationTemplate> builder)
    {
        builder.ToTable("CommunicationTemplates");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(t => t.SubjectTemplate)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(t => t.BodyTemplate)
            .IsRequired();

        builder.Property(t => t.Language)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(t => t.Category)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(t => t.Version)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(t => t.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.HasIndex(t => new { t.TenantId, t.Category, t.Language })
            .HasDatabaseName("IX_CommunicationTemplate_TenantId_Category_Language");

        builder.HasIndex(t => new { t.TenantId, t.Name })
            .HasDatabaseName("IX_CommunicationTemplate_TenantId_Name");
    }
}
