namespace Itdg.Crm.Api.Test.Queries;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Application.QueryHandlers;
using Microsoft.Extensions.Logging;

public class SearchDocumentsHandlerTests
{
    private readonly ISearchService _searchService;
    private readonly ILogger<SearchDocumentsHandler> _logger;
    private readonly SearchDocumentsHandler _handler;

    public SearchDocumentsHandlerTests()
    {
        _searchService = Substitute.For<ISearchService>();
        _logger = Substitute.For<ILogger<SearchDocumentsHandler>>();
        _handler = new SearchDocumentsHandler(_searchService, _logger);
    }

    [Fact]
    public async Task HandleAsync_ReturnsPaginatedResult_WithCorrectData()
    {
        // Arrange
        var items = new List<DocumentSearchResultDto>
        {
            new(Guid.NewGuid(), Guid.NewGuid(), "Client A", "file1.pdf", "Tax Returns", DateTimeOffset.UtcNow, "...tax return snippet..."),
            new(Guid.NewGuid(), Guid.NewGuid(), "Client B", "file2.pdf", "Bank Statements", DateTimeOffset.UtcNow, "...bank statement snippet...")
        };

        _searchService.SearchDocumentsAsync(
            "tax", null, null, null, null, 1, 20, Arg.Any<CancellationToken>())
            .Returns((items.AsReadOnly() as IReadOnlyList<DocumentSearchResultDto>, 2));

        var query = new SearchDocuments("tax");

        // Act
        var result = await _handler.HandleAsync(query, Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task HandleAsync_PassesFilters_ToSearchService()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var dateFrom = DateTimeOffset.UtcNow.AddDays(-30);
        var dateTo = DateTimeOffset.UtcNow;

        _searchService.SearchDocumentsAsync(
            "tax", clientId, "Tax Returns", dateFrom, dateTo, 2, 10, Arg.Any<CancellationToken>())
            .Returns((new List<DocumentSearchResultDto>().AsReadOnly() as IReadOnlyList<DocumentSearchResultDto>, 0));

        var query = new SearchDocuments("tax", clientId, "Tax Returns", dateFrom, dateTo, 2, 10);

        // Act
        var result = await _handler.HandleAsync(query, Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _searchService.Received(1).SearchDocumentsAsync(
            "tax", clientId, "Tax Returns", dateFrom, dateTo, 2, 10, Arg.Any<CancellationToken>());
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_ReturnsEmptyResult_WhenNoDocumentsFound()
    {
        // Arrange
        _searchService.SearchDocumentsAsync(
            "nonexistent", null, null, null, null, 1, 20, Arg.Any<CancellationToken>())
            .Returns((new List<DocumentSearchResultDto>().AsReadOnly() as IReadOnlyList<DocumentSearchResultDto>, 0));

        var query = new SearchDocuments("nonexistent");

        // Act
        var result = await _handler.HandleAsync(query, Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_MapsSearchResultProperties_Correctly()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var uploadedAt = DateTimeOffset.UtcNow.AddDays(-5);

        var items = new List<DocumentSearchResultDto>
        {
            new(documentId, clientId, "Rodriguez & Associates", "tax-return-2024.pdf", "Tax Returns", uploadedAt, "...relevant content snippet...")
        };

        _searchService.SearchDocumentsAsync(
            "tax return", null, null, null, null, 1, 20, Arg.Any<CancellationToken>())
            .Returns((items.AsReadOnly() as IReadOnlyList<DocumentSearchResultDto>, 1));

        var query = new SearchDocuments("tax return");

        // Act
        var result = await _handler.HandleAsync(query, Guid.NewGuid(), CancellationToken.None);

        // Assert
        var dto = result.Items.First();
        dto.DocumentId.Should().Be(documentId);
        dto.ClientId.Should().Be(clientId);
        dto.ClientName.Should().Be("Rodriguez & Associates");
        dto.FileName.Should().Be("tax-return-2024.pdf");
        dto.Category.Should().Be("Tax Returns");
        dto.UploadedAt.Should().Be(uploadedAt);
        dto.RelevanceSnippet.Should().Be("...relevant content snippet...");
    }

    [Fact]
    public async Task HandleAsync_SearchWithOnlyQuery_PassesNullFilters()
    {
        // Arrange
        _searchService.SearchDocumentsAsync(
            "invoice", null, null, null, null, 1, 20, Arg.Any<CancellationToken>())
            .Returns((new List<DocumentSearchResultDto>().AsReadOnly() as IReadOnlyList<DocumentSearchResultDto>, 0));

        var query = new SearchDocuments("invoice");

        // Act
        await _handler.HandleAsync(query, Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _searchService.Received(1).SearchDocumentsAsync(
            "invoice", null, null, null, null, 1, 20, Arg.Any<CancellationToken>());
    }
}
