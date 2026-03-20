namespace Itdg.Crm.Api.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class DocumentCategoryConfiguration : IEntityTypeConfiguration<DocumentCategory>
{
    public static readonly Guid BankStatementsId = Guid.Parse("d0000001-0000-0000-0000-000000000001");
    public static readonly Guid InvoicesId = Guid.Parse("d0000001-0000-0000-0000-000000000002");
    public static readonly Guid ReportsId = Guid.Parse("d0000001-0000-0000-0000-000000000003");
    public static readonly Guid TaxDocumentsId = Guid.Parse("d0000001-0000-0000-0000-000000000004");
    public static readonly Guid ContractsId = Guid.Parse("d0000001-0000-0000-0000-000000000005");
    public static readonly Guid GeneralId = Guid.Parse("d0000001-0000-0000-0000-000000000006");

    private static readonly DateTimeOffset SeedDate = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

    public void Configure(EntityTypeBuilder<DocumentCategory> builder)
    {
        builder.ToTable("DocumentCategories");

        builder.HasKey(dc => dc.Id);

        builder.Property(dc => dc.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(dc => dc.NamingConvention)
            .HasMaxLength(200);

        builder.Property(dc => dc.IsDefault)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(dc => new { dc.TenantId, dc.Name })
            .IsUnique()
            .HasDatabaseName("IX_DocumentCategory_TenantId_Name");

        builder.HasData(
            CreateSeed(BankStatementsId, "Bank Statements", "{ClientName}_BankStatement_{Date}"),
            CreateSeed(InvoicesId, "Invoices", "{ClientName}_Invoice_{Date}"),
            CreateSeed(ReportsId, "Reports", "{ClientName}_Report_{Date}"),
            CreateSeed(TaxDocumentsId, "Tax Documents", "{ClientName}_TaxDoc_{Date}"),
            CreateSeed(ContractsId, "Contracts", "{ClientName}_Contract_{Date}"),
            CreateSeed(GeneralId, "General", "{ClientName}_General_{Date}")
        );
    }

    private static DocumentCategory CreateSeed(Guid id, string name, string namingConvention)
    {
        return new DocumentCategory
        {
            Id = id,
            TenantId = TenantConfiguration.DefaultTenantId,
            Name = name,
            NamingConvention = namingConvention,
            IsDefault = true,
            CreatedAt = SeedDate,
            UpdatedAt = SeedDate
        };
    }
}
