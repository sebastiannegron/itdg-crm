namespace Itdg.Crm.Api.Infrastructure.Data.Configurations;

using Itdg.Crm.Api.Domain.GeneralConstants;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ClientPortalInvitationConfiguration : IEntityTypeConfiguration<ClientPortalInvitation>
{
    public void Configure(EntityTypeBuilder<ClientPortalInvitation> builder)
    {
        builder.ToTable("ClientPortalInvitations");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(i => i.Token)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(i => i.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasOne(i => i.Client)
            .WithMany()
            .HasForeignKey(i => i.ClientId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(i => i.Token)
            .IsUnique()
            .HasDatabaseName("IX_ClientPortalInvitation_Token");

        builder.HasIndex(i => new { i.TenantId, i.ClientId })
            .HasDatabaseName("IX_ClientPortalInvitation_TenantId_ClientId");
    }
}
