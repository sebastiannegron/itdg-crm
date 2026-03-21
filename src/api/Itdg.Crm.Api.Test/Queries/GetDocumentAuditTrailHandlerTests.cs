namespace Itdg.Crm.Api.Test.Queries;

using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Application.QueryHandlers;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetDocumentAuditTrailHandlerTests
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly ILogger<GetDocumentAuditTrailHandler> _logger;
    private readonly GetDocumentAuditTrailHandler _handler;

    public GetDocumentAuditTrailHandlerTests()
    {
        _auditLogRepository = Substitute.For<IAuditLogRepository>();
        _documentRepository = Substitute.For<IDocumentRepository>();
        _logger = Substitute.For<ILogger<GetDocumentAuditTrailHandler>>();
        _handler = new GetDocumentAuditTrailHandler(_auditLogRepository, _documentRepository, _logger);
    }

    [Fact]
    public async Task HandleAsync_ReturnsPaginatedAuditLogs()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var document = new Document
        {
            Id = documentId,
            ClientId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            FileName = "test.pdf",
            GoogleDriveFileId = "drive-id-1",
            UploadedById = Guid.NewGuid(),
            CurrentVersion = 1,
            FileSize = 1024,
            MimeType = "application/pdf",
            TenantId = Guid.NewGuid()
        };

        _documentRepository.GetByIdAsync(documentId, Arg.Any<CancellationToken>())
            .Returns(document);

        var auditLogs = new List<AuditLog>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                EntityType = "Document",
                EntityId = documentId,
                Action = "Download",
                Timestamp = DateTimeOffset.UtcNow.AddMinutes(-10),
                IpAddress = "192.168.1.1"
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                EntityType = "Document",
                EntityId = documentId,
                Action = "View",
                Timestamp = DateTimeOffset.UtcNow.AddMinutes(-5),
                IpAddress = "192.168.1.2"
            }
        };

        _auditLogRepository.GetByEntityAsync("Document", documentId, 1, 20, Arg.Any<CancellationToken>())
            .Returns((auditLogs.AsReadOnly(), 2));

        var query = new GetDocumentAuditTrail(documentId, Page: 1, PageSize: 20);

        // Act
        var result = await _handler.HandleAsync(query, Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);

        var firstItem = result.Items.First();
        firstItem.EntityType.Should().Be("Document");
        firstItem.EntityId.Should().Be(documentId);
        firstItem.Action.Should().Be("Download");
        firstItem.IpAddress.Should().Be("192.168.1.1");
    }

    [Fact]
    public async Task HandleAsync_ThrowsNotFoundException_WhenDocumentDoesNotExist()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        _documentRepository.GetByIdAsync(documentId, Arg.Any<CancellationToken>())
            .Returns((Document?)null);

        var query = new GetDocumentAuditTrail(documentId);

        // Act
        var act = () => _handler.HandleAsync(query, Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{documentId}*");
    }

    [Fact]
    public async Task HandleAsync_ReturnsEmptyResult_WhenNoAuditLogs()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var document = new Document
        {
            Id = documentId,
            ClientId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            FileName = "test.pdf",
            GoogleDriveFileId = "drive-id-1",
            UploadedById = Guid.NewGuid(),
            CurrentVersion = 1,
            FileSize = 1024,
            MimeType = "application/pdf",
            TenantId = Guid.NewGuid()
        };

        _documentRepository.GetByIdAsync(documentId, Arg.Any<CancellationToken>())
            .Returns(document);

        _auditLogRepository.GetByEntityAsync("Document", documentId, 1, 20, Arg.Any<CancellationToken>())
            .Returns((new List<AuditLog>().AsReadOnly(), 0));

        var query = new GetDocumentAuditTrail(documentId, Page: 1, PageSize: 20);

        // Act
        var result = await _handler.HandleAsync(query, Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }
}
