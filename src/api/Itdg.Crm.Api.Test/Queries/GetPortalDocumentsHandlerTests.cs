namespace Itdg.Crm.Api.Test.Queries;

using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Application.QueryHandlers;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetPortalDocumentsHandlerTests
{
    private readonly IDocumentRepository _documentRepository;
    private readonly ILogger<GetPortalDocumentsHandler> _logger;
    private readonly GetPortalDocumentsHandler _handler;

    public GetPortalDocumentsHandlerTests()
    {
        _documentRepository = Substitute.For<IDocumentRepository>();
        _logger = Substitute.For<ILogger<GetPortalDocumentsHandler>>();
        _handler = new GetPortalDocumentsHandler(_documentRepository, _logger);
    }

    [Fact]
    public async Task HandleAsync_ReturnsPaginatedDocuments_ForClient()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var query = new GetPortalDocuments(clientId);

        var documents = new List<Document>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                CategoryId = categoryId,
                Category = new DocumentCategory { Id = categoryId, Name = "Tax Documents", SortOrder = 1, TenantId = Guid.NewGuid() },
                FileName = "tax-return-2025.pdf",
                GoogleDriveFileId = "drive-file-1",
                UploadedById = Guid.NewGuid(),
                CurrentVersion = 1,
                FileSize = 2048,
                MimeType = "application/pdf",
                TenantId = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                CategoryId = categoryId,
                Category = new DocumentCategory { Id = categoryId, Name = "Tax Documents", SortOrder = 1, TenantId = Guid.NewGuid() },
                FileName = "w2-form.pdf",
                GoogleDriveFileId = "drive-file-2",
                UploadedById = Guid.NewGuid(),
                CurrentVersion = 2,
                FileSize = 1024,
                MimeType = "application/pdf",
                TenantId = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            }
        };

        _documentRepository.GetPagedByClientIdAsync(
                clientId, 1, 20, null, null, null, Arg.Any<CancellationToken>())
            .Returns((documents.AsReadOnly(), 2));

        // Act
        var result = await _handler.HandleAsync(query, Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
        result.Items[0].FileName.Should().Be("tax-return-2025.pdf");
        result.Items[0].CategoryName.Should().Be("Tax Documents");
        result.Items[1].FileName.Should().Be("w2-form.pdf");
        result.Items[1].CurrentVersion.Should().Be(2);
    }

    [Fact]
    public async Task HandleAsync_ReturnsEmptyResult_WhenNoDocuments()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var query = new GetPortalDocuments(clientId);

        _documentRepository.GetPagedByClientIdAsync(
                clientId, 1, 20, null, null, null, Arg.Any<CancellationToken>())
            .Returns((new List<Document>().AsReadOnly(), 0));

        // Act
        var result = await _handler.HandleAsync(query, Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_PassesFilterParameters_ToRepository()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var query = new GetPortalDocuments(clientId, Page: 2, PageSize: 10, CategoryId: categoryId, Year: 2025, Search: "tax");

        _documentRepository.GetPagedByClientIdAsync(
                clientId, 2, 10, categoryId, 2025, "tax", Arg.Any<CancellationToken>())
            .Returns((new List<Document>().AsReadOnly(), 0));

        // Act
        await _handler.HandleAsync(query, Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _documentRepository.Received(1).GetPagedByClientIdAsync(
            clientId, 2, 10, categoryId, 2025, "tax", Arg.Any<CancellationToken>());
    }
}
