namespace Itdg.Crm.Api.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ClientAssignmentConfiguration : IEntityTypeConfiguration<ClientAssignment>
{
    public void Configure(EntityTypeBuilder<ClientAssignment> builder)
    {
        builder.ToTable("ClientAssignments");

        builder.HasKey(ca => ca.Id);

        builder.HasOne(ca => ca.Client)
            .WithMany()
            .HasForeignKey(ca => ca.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ca => ca.User)
            .WithMany()
            .HasForeignKey(ca => ca.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(ca => ca.AssignedAt)
            .IsRequired();

        builder.HasIndex(ca => new { ca.TenantId, ca.ClientId, ca.UserId })
            .IsUnique()
            .HasDatabaseName("IX_ClientAssignment_TenantId_ClientId_UserId");

        builder.HasIndex(ca => new { ca.TenantId, ca.UserId })
            .HasDatabaseName("IX_ClientAssignment_TenantId_UserId");
    }
}
