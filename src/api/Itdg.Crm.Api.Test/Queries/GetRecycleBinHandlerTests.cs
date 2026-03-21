namespace Itdg.Crm.Api.Test.Queries;

using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Application.QueryHandlers;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetRecycleBinHandlerTests
{
    private readonly IDocumentRepository _documentRepository;
    private readonly ILogger<GetRecycleBinHandler> _logger;
    private readonly GetRecycleBinHandler _handler;

    public GetRecycleBinHandlerTests()
    {
        _documentRepository = Substitute.For<IDocumentRepository>();
        _logger = Substitute.For<ILogger<GetRecycleBinHandler>>();
        _handler = new GetRecycleBinHandler(_documentRepository, _logger);
    }

    [Fact]
    public async Task HandleAsync_ReturnsPaginatedDeletedDocuments()
    {
        // Arrange
        var deletedAt = DateTimeOffset.UtcNow.AddDays(-3);
        var documents = new List<Document>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ClientId = Guid.NewGuid(),
                CategoryId = Guid.NewGuid(),
                Category = new DocumentCategory { Id = Guid.NewGuid(), Name = "Tax Documents", SortOrder = 1, TenantId = Guid.NewGuid() },
                FileName = "deleted-doc.pdf",
                GoogleDriveFileId = "drive-id-1",
                UploadedById = Guid.NewGuid(),
                CurrentVersion = 1,
                FileSize = 1024,
                MimeType = "application/pdf",
                TenantId = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-10),
                DeletedAt = deletedAt
            }
        };

        _documentRepository.GetDeletedDocumentsPagedAsync(1, 20, Arg.Any<CancellationToken>())
            .Returns((documents.AsReadOnly(), 1));

        var query = new GetRecycleBin(Page: 1, PageSize: 20);

        // Act
        var result = await _handler.HandleAsync(query, Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);

        var item = result.Items.First();
        item.DocumentId.Should().Be(documents[0].Id);
        item.FileName.Should().Be("deleted-doc.pdf");
        item.CategoryName.Should().Be("Tax Documents");
        item.DeletedAt.Should().Be(deletedAt);
    }

    [Fact]
    public async Task HandleAsync_ReturnsEmptyResult_WhenNoDeletedDocuments()
    {
        // Arrange
        _documentRepository.GetDeletedDocumentsPagedAsync(1, 20, Arg.Any<CancellationToken>())
            .Returns((new List<Document>().AsReadOnly(), 0));

        var query = new GetRecycleBin(Page: 1, PageSize: 20);

        // Act
        var result = await _handler.HandleAsync(query, Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}
