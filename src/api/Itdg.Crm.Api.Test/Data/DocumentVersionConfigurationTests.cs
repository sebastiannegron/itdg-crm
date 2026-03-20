namespace Itdg.Crm.Api.Test.Data;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Infrastructure.Data;

public class DocumentVersionConfigurationTests
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
    public void DocumentVersionConfiguration_HasCorrectTableName()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(DocumentVersion));

        // Assert
        entityType.Should().NotBeNull();
        entityType!.GetTableName().Should().Be("DocumentVersions");
    }

    [Fact]
    public void DocumentVersionConfiguration_HasPrimaryKey()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(DocumentVersion));

        // Assert
        entityType.Should().NotBeNull();
        var primaryKey = entityType!.FindPrimaryKey();
        primaryKey.Should().NotBeNull();
        primaryKey!.Properties.Should().ContainSingle(p => p.Name == "Id");
    }

    [Fact]
    public void DocumentVersionConfiguration_GoogleDriveFileIdProperty_IsRequired_HasMaxLength256()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(DocumentVersion));
        var property = entityType!.FindProperty(nameof(DocumentVersion.GoogleDriveFileId));

        // Assert
        property.Should().NotBeNull();
        property!.IsNullable.Should().BeFalse();
        property.GetMaxLength().Should().Be(256);
    }

    [Fact]
    public void DocumentVersionConfiguration_HasUniqueDocumentIdVersionNumberIndex()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(DocumentVersion));
        var indexes = entityType!.GetIndexes().ToList();

        // Assert
        var index = indexes
            .FirstOrDefault(i => i.Properties.Any(p => p.Name == "DocumentId") &&
                                  i.Properties.Any(p => p.Name == "VersionNumber") &&
                                  i.Properties.Count == 2);
        index.Should().NotBeNull();
        index!.IsUnique.Should().BeTrue();
        index.GetDatabaseName().Should().Be("IX_DocumentVersion_DocumentId_VersionNumber");
    }

    [Fact]
    public void DocumentVersionConfiguration_HasDocumentForeignKey()
    {
        // Arrange
        using var context = CreateContext(Guid.NewGuid().ToString());

        // Act
        var entityType = context.Model.FindEntityType(typeof(DocumentVersion));
        var foreignKeys = entityType!.GetForeignKeys().ToList();

        // Assert
        var documentFk = foreignKeys
            .FirstOrDefault(fk => fk.Properties.Any(p => p.Name == "DocumentId"));
        documentFk.Should().NotBeNull();
        documentFk!.DeleteBehavior.Should().Be(DeleteBehavior.Cascade);
    }

    [Fact]
    public async Task DocumentVersion_CanBeSavedAndRetrieved()
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
        var documentId = Guid.NewGuid();
        var uploadedAt = DateTimeOffset.UtcNow;

        var category = new DocumentCategory
        {
            Id = categoryId,
            TenantId = tenantId,
            Name = "General",
            IsDefault = true
        };

        var document = new Document
        {
            Id = documentId,
            TenantId = tenantId,
            ClientId = Guid.NewGuid(),
            CategoryId = categoryId,
            FileName = "report.pdf",
            GoogleDriveFileId = "gdrive-v1",
            UploadedById = Guid.NewGuid(),
            CurrentVersion = 2,
            FileSize = 512000,
            MimeType = "application/pdf"
        };

        var version = new DocumentVersion
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            VersionNumber = 1,
            GoogleDriveFileId = "gdrive-v1",
            UploadedById = Guid.NewGuid(),
            UploadedAt = uploadedAt
        };

        // Act
        using (var seedContext = new TestCrmDbContext(options, tenantProvider))
        {
            seedContext.DocumentCategories.Add(category);
            seedContext.Documents.Add(document);
            seedContext.DocumentVersions.Add(version);
            await seedContext.SaveChangesAsync();
        }

        using var queryContext = new TestCrmDbContext(options, tenantProvider);
        var result = await queryContext.DocumentVersions
            .Include(dv => dv.Document)
            .FirstOrDefaultAsync();

        // Assert
        result.Should().NotBeNull();
        result!.VersionNumber.Should().Be(1);
        result.GoogleDriveFileId.Should().Be("gdrive-v1");
        result.UploadedAt.Should().Be(uploadedAt);
        result.Document.Should().NotBeNull();
        result.Document!.FileName.Should().Be("report.pdf");
    }
}
