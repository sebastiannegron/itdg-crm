namespace Itdg.Crm.Api.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class DashboardLayoutConfiguration : IEntityTypeConfiguration<DashboardLayout>
{
    public void Configure(EntityTypeBuilder<DashboardLayout> builder)
    {
        builder.ToTable("DashboardLayouts");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.UserId)
            .IsRequired();

        builder.Property(d => d.WidgetConfigurations)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(d => new { d.TenantId, d.UserId })
            .IsUnique()
            .HasDatabaseName("IX_DashboardLayout_TenantId_UserId");
    }
}
