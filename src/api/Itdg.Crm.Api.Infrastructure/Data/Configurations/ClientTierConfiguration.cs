namespace Itdg.Crm.Api.Infrastructure.Data.Configurations;

using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ClientTierConfiguration : IEntityTypeConfiguration<ClientTier>
{
    public void Configure(EntityTypeBuilder<ClientTier> builder)
    {
        builder.ToTable("ClientTiers");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasData(
            new ClientTier
            {
                Id = new Guid("00000000-0000-0000-0000-000000000001"),
                Name = "Tier 1",
                SortOrder = 1,
                TenantId = SeedConstants.DefaultTenantId,
                CreatedAt = SeedConstants.SeedTimestamp,
                UpdatedAt = SeedConstants.SeedTimestamp
            },
            new ClientTier
            {
                Id = new Guid("00000000-0000-0000-0000-000000000002"),
                Name = "Tier 2",
                SortOrder = 2,
                TenantId = SeedConstants.DefaultTenantId,
                CreatedAt = SeedConstants.SeedTimestamp,
                UpdatedAt = SeedConstants.SeedTimestamp
            },
            new ClientTier
            {
                Id = new Guid("00000000-0000-0000-0000-000000000003"),
                Name = "Tier 3",
                SortOrder = 3,
                TenantId = SeedConstants.DefaultTenantId,
                CreatedAt = SeedConstants.SeedTimestamp,
                UpdatedAt = SeedConstants.SeedTimestamp
            }
        );
    }
}
