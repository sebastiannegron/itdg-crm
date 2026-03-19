namespace Itdg.Crm.Api.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public static readonly Guid DefaultTenantId = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890");

    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("Tenants");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(t => t.Subdomain)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(t => t.Settings)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(t => t.Subdomain)
            .IsUnique()
            .HasDatabaseName("IX_Tenant_Subdomain");

        builder.HasData(new Tenant
        {
            Id = DefaultTenantId,
            Name = "Development Tenant",
            Subdomain = "dev",
            Settings = null,
            CreatedAt = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            UpdatedAt = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero)
        });
    }
}
