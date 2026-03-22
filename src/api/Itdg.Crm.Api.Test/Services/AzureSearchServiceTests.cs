namespace Itdg.Crm.Api.Test.Services;

using Azure.Search.Documents.Indexes.Models;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Infrastructure.Options;
using Itdg.Crm.Api.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class AzureSearchServiceTests
{
    private readonly IOptions<AzureSearchOptions> _options;
    private readonly ILogger<AzureSearchService> _logger;

    public AzureSearchServiceTests()
    {
        var azureSearchOptions = new AzureSearchOptions
        {
            Endpoint = "https://test-search.search.windows.net",
            ApiKey = "test-api-key",
            IndexName = "documents"
        };
        _options = Options.Create(azureSearchOptions);
        _logger = Substitute.For<ILogger<AzureSearchService>>();
    }

    [Fact]
    public void Constructor_WithValidOptions_CreatesInstance()
    {
        // Act
        var service = new AzureSearchService(_options, _logger);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void CreateSearchClient_ReturnsConfiguredClient()
    {
        // Arrange
        var service = new AzureSearchService(_options, _logger);

        // Act
        var client = service.CreateSearchClient();

        // Assert
        client.Should().NotBeNull();
        client.IndexName.Should().Be("documents");
    }

    [Fact]
    public void CreateIndexDefinition_ReturnsIndexWithCorrectName()
    {
        // Act
        SearchIndex index = AzureSearchService.CreateIndexDefinition("documents");

        // Assert
        index.Name.Should().Be("documents");
    }

    [Fact]
    public void CreateIndexDefinition_ContainsAllRequiredFields()
    {
        // Act
        SearchIndex index = AzureSearchService.CreateIndexDefinition("documents");

        // Assert
        index.Fields.Should().HaveCount(7);
        index.Fields.Select(f => f.Name).Should().Contain(new[]
        {
            "documentId", "clientId", "clientName", "fileName", "category", "content", "uploadedAt"
        });
    }

    [Fact]
    public void CreateIndexDefinition_DocumentIdIsKeyField()
    {
        // Act
        SearchIndex index = AzureSearchService.CreateIndexDefinition("documents");

        // Assert
        var keyField = index.Fields.First(f => f.Name == "documentId");
        keyField.IsKey.Should().BeTrue();
        keyField.IsFilterable.Should().BeTrue();
    }

    [Fact]
    public void CreateIndexDefinition_ClientIdIsFilterable()
    {
        // Act
        SearchIndex index = AzureSearchService.CreateIndexDefinition("documents");

        // Assert
        var field = index.Fields.First(f => f.Name == "clientId");
        field.IsFilterable.Should().BeTrue();
    }

    [Fact]
    public void CreateIndexDefinition_SearchableFieldsAreCorrectlyConfigured()
    {
        // Act
        SearchIndex index = AzureSearchService.CreateIndexDefinition("documents");

        // Assert
        var clientNameField = index.Fields.First(f => f.Name == "clientName");
        clientNameField.IsSearchable.Should().BeTrue();
        clientNameField.IsFilterable.Should().BeTrue();
        clientNameField.IsSortable.Should().BeTrue();

        var fileNameField = index.Fields.First(f => f.Name == "fileName");
        fileNameField.IsSearchable.Should().BeTrue();
        fileNameField.IsFilterable.Should().BeTrue();
        fileNameField.IsSortable.Should().BeTrue();

        var categoryField = index.Fields.First(f => f.Name == "category");
        categoryField.IsSearchable.Should().BeTrue();
        categoryField.IsFilterable.Should().BeTrue();
        categoryField.IsSortable.Should().BeTrue();

        var contentField = index.Fields.First(f => f.Name == "content");
        contentField.IsSearchable.Should().BeTrue();
    }

    [Fact]
    public void CreateIndexDefinition_UploadedAtIsFilterableAndSortable()
    {
        // Act
        SearchIndex index = AzureSearchService.CreateIndexDefinition("documents");

        // Assert
        var field = index.Fields.First(f => f.Name == "uploadedAt");
        field.IsFilterable.Should().BeTrue();
        field.IsSortable.Should().BeTrue();
    }

    [Fact]
    public void MapToIndexDocument_MapsAllProperties()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var uploadedAt = DateTimeOffset.UtcNow;
        var dto = new SearchDocumentDto(documentId, clientId, "Acme Corp", "tax-return.pdf", "Tax Documents", "Some content", uploadedAt);

        // Act
        var result = AzureSearchService.MapToIndexDocument(dto);

        // Assert
        result.DocumentId.Should().Be(documentId.ToString());
        result.ClientId.Should().Be(clientId.ToString());
        result.ClientName.Should().Be("Acme Corp");
        result.FileName.Should().Be("tax-return.pdf");
        result.Category.Should().Be("Tax Documents");
        result.Content.Should().Be("Some content");
        result.UploadedAt.Should().Be(uploadedAt);
    }

    [Fact]
    public void MapToIndexDocument_HandlesNullContent()
    {
        // Arrange
        var dto = new SearchDocumentDto(Guid.NewGuid(), Guid.NewGuid(), "Client", "file.pdf", "General", null, DateTimeOffset.UtcNow);

        // Act
        var result = AzureSearchService.MapToIndexDocument(dto);

        // Assert
        result.Content.Should().BeNull();
    }

    [Fact]
    public void MapFromIndexDocument_MapsAllProperties()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var uploadedAt = DateTimeOffset.UtcNow;
        var doc = new SearchIndexDocument
        {
            DocumentId = documentId.ToString(),
            ClientId = clientId.ToString(),
            ClientName = "Acme Corp",
            FileName = "tax-return.pdf",
            Category = "Tax Documents",
            Content = "Some content",
            UploadedAt = uploadedAt
        };

        // Act
        var result = AzureSearchService.MapFromIndexDocument(doc);

        // Assert
        result.DocumentId.Should().Be(documentId);
        result.ClientId.Should().Be(clientId);
        result.ClientName.Should().Be("Acme Corp");
        result.FileName.Should().Be("tax-return.pdf");
        result.Category.Should().Be("Tax Documents");
        result.Content.Should().Be("Some content");
        result.UploadedAt.Should().Be(uploadedAt);
    }

    [Fact]
    public void MapFromIndexDocument_HandlesNullContent()
    {
        // Arrange
        var doc = new SearchIndexDocument
        {
            DocumentId = Guid.NewGuid().ToString(),
            ClientId = Guid.NewGuid().ToString(),
            ClientName = "Client",
            FileName = "file.pdf",
            Category = "General",
            Content = null,
            UploadedAt = DateTimeOffset.UtcNow
        };

        // Act
        var result = AzureSearchService.MapFromIndexDocument(doc);

        // Assert
        result.Content.Should().BeNull();
    }

    [Fact]
    public void MapRoundTrip_PreservesAllData()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var uploadedAt = DateTimeOffset.UtcNow;
        var original = new SearchDocumentDto(documentId, clientId, "Acme Corp", "tax-return.pdf", "Tax Documents", "Content text", uploadedAt);

        // Act
        var indexed = AzureSearchService.MapToIndexDocument(original);
        var roundTripped = AzureSearchService.MapFromIndexDocument(indexed);

        // Assert
        roundTripped.Should().BeEquivalentTo(original);
    }
}
