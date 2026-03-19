namespace Itdg.Crm.Api.Infrastructure.Data.Configurations;

using Itdg.Crm.Api.Domain.GeneralConstants;
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
    }
}
