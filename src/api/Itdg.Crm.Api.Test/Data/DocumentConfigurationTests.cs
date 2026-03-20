namespace Itdg.Crm.Api.Test.Data;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Infrastructure.Data;
using Itdg.Crm.Api.Infrastructure.Data.Configurations;

public class DocumentConfigurationTests
{
    private class TestCrmDbContext : CrmDbContext
    {
        public TestCrmDbContext(DbContextOptions<TestCrmDbContext> options, ITenantProvider tenantProvider)
            : base(options, tenantProvider)
        {
        }
    }

    private static TestCrmDbContext CreateContext(string databaseName)
    {
        var tenantProvider = Substitute.For<ITenantProvider>();
        tenantProvider.GetTenantId().Returns(Guid.NewGuid());

        var options = new DbContextOptionsBuilder<TestCrmDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new TestCrmDbContext(options, tenantProvider);
    }

    [Fact]
    public void DocumentConfiguration_HasCorrectTableName()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(Document));

        // Assert
        entityType.Should().NotBeNull();
        entityType!.GetTableName().Should().Be("Documents");
    }

    [Fact]
    public void DocumentConfiguration_HasPrimaryKey()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(Document));

        // Assert
        entityType.Should().NotBeNull();
        var primaryKey = entityType!.FindPrimaryKey();
        primaryKey.Should().NotBeNull();
        primaryKey!.Properties.Should().ContainSingle(p => p.Name == "Id");
    }

    [Fact]
    public void DocumentConfiguration_FileNameProperty_IsRequired_HasMaxLength500()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(Document));
        var fileNameProperty = entityType!.FindProperty(nameof(Document.FileName));

        // Assert
        fileNameProperty.Should().NotBeNull();
        fileNameProperty!.IsNullable.Should().BeFalse();
        fileNameProperty.GetMaxLength().Should().Be(500);
    }

    [Fact]
    public void DocumentConfiguration_GoogleDriveFileIdProperty_IsRequired_HasMaxLength256()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(Document));
        var property = entityType!.FindProperty(nameof(Document.GoogleDriveFileId));

        // Assert
        property.Should().NotBeNull();
        property!.IsNullable.Should().BeFalse();
        property.GetMaxLength().Should().Be(256);
    }

    [Fact]
    public void DocumentConfiguration_MimeTypeProperty_IsRequired_HasMaxLength256()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(Document));
        var property = entityType!.FindProperty(nameof(Document.MimeType));

        // Assert
        property.Should().NotBeNull();
        property!.IsNullable.Should().BeFalse();
        property.GetMaxLength().Should().Be(256);
    }

    [Fact]
    public void DocumentConfiguration_HasTenantIdClientIdCategoryIdIndex()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(Document));
        var indexes = entityType!.GetIndexes().ToList();

        // Assert
        var index = indexes
            .FirstOrDefault(i => i.Properties.Any(p => p.Name == "TenantId") &&
                                  i.Properties.Any(p => p.Name == "ClientId") &&
                                  i.Properties.Any(p => p.Name == "CategoryId") &&
                                  i.Properties.Count == 3);
        index.Should().NotBeNull();
        index!.GetDatabaseName().Should().Be("IX_Document_TenantId_ClientId_CategoryId");
    }

    [Fact]
    public void DocumentConfiguration_HasCategoryForeignKey()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(Document));
        var foreignKeys = entityType!.GetForeignKeys().ToList();

        // Assert
        var categoryFk = foreignKeys
            .FirstOrDefault(fk => fk.Properties.Any(p => p.Name == "CategoryId"));
        categoryFk.Should().NotBeNull();
        categoryFk!.DeleteBehavior.Should().Be(DeleteBehavior.Restrict);
    }

    [Fact]
    public async Task Document_CanBeSavedAndRetrieved()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        var tenantProvider = Substitute.For<ITenantProvider>();
        tenantProvider.GetTenantId().Returns(tenantId);

        var options = new DbContextOptionsBuilder<TestCrmDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;

        var categoryId = Guid.NewGuid();
        var category = new DocumentCategory
        {
            Id = categoryId,
            TenantId = tenantId,
            Name = "Tax Documents",
            NamingConvention = "{ClientName}_TaxDoc_{Date}",
            IsDefault = true
        };

        var document = new Document
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ClientId = Guid.NewGuid(),
            CategoryId = categoryId,
            FileName = "tax_return_2024.pdf",
            GoogleDriveFileId = "gdrive-abc123",
            UploadedById = Guid.NewGuid(),
            CurrentVersion = 1,
            FileSize = 1024000,
            MimeType = "application/pdf"
        };

        // Act
        using (var seedContext = new TestCrmDbContext(options, tenantProvider))
        {
            seedContext.DocumentCategories.Add(category);
            seedContext.Documents.Add(document);
            await seedContext.SaveChangesAsync();
        }

        using var queryContext = new TestCrmDbContext(options, tenantProvider);
        var result = await queryContext.Documents
            .Include(d => d.Category)
            .FirstOrDefaultAsync();

        // Assert
        result.Should().NotBeNull();
        result!.FileName.Should().Be("tax_return_2024.pdf");
        result.GoogleDriveFileId.Should().Be("gdrive-abc123");
        result.MimeType.Should().Be("application/pdf");
        result.CurrentVersion.Should().Be(1);
        result.FileSize.Should().Be(1024000);
        result.Category.Should().NotBeNull();
        result.Category!.Name.Should().Be("Tax Documents");
    }
}
