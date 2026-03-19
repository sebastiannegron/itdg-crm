namespace Itdg.Crm.Api.Infrastructure.Data.Configurations;

using Itdg.Crm.Api.Domain.GeneralConstants;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("Clients");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.ContactEmail)
            .HasMaxLength(256);

        builder.Property(c => c.Phone)
            .HasMaxLength(50);

        builder.Property(c => c.Address)
            .HasMaxLength(500);

        builder.Property(c => c.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(c => c.IndustryTag)
            .HasMaxLength(100);

        builder.Property(c => c.CustomFields)
            .HasColumnType("nvarchar(max)");

        builder.HasOne(c => c.Tier)
            .WithMany()
            .HasForeignKey(c => c.TierId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(c => new { c.TenantId, c.Status })
            .HasDatabaseName("IX_Client_TenantId_Status");
    }
}
