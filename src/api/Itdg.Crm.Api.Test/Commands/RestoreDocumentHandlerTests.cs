namespace Itdg.Crm.Api.Test.Commands;

using Itdg.Crm.Api.Application.CommandHandlers;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class RestoreDocumentHandlerTests
{
    private readonly IDocumentRepository _repository;
    private readonly ILogger<RestoreDocumentHandler> _logger;
    private readonly RestoreDocumentHandler _handler;

    public RestoreDocumentHandlerTests()
    {
        _repository = Substitute.For<IDocumentRepository>();
        _logger = Substitute.For<ILogger<RestoreDocumentHandler>>();
        _handler = new RestoreDocumentHandler(_repository, _logger);
    }

    [Fact]
    public async Task HandleAsync_RestoresDocument_WhenDocumentIsDeleted()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var deletedDocument = new Document
        {
            Id = documentId,
            ClientId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            FileName = "test.pdf",
            GoogleDriveFileId = "drive-id",
            UploadedById = Guid.NewGuid(),
            CurrentVersion = 1,
            FileSize = 1024,
            MimeType = "application/pdf",
            TenantId = Guid.NewGuid(),
            DeletedAt = DateTimeOffset.UtcNow.AddDays(-5)
        };

        _repository.GetByIdIncludingDeletedAsync(documentId, Arg.Any<CancellationToken>()).Returns(deletedDocument);

        var command = new RestoreDocument(DocumentId: documentId);

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _repository.Received(1).UpdateAsync(Arg.Is<Document>(d =>
            d.Id == documentId &&
            d.DeletedAt == null
        ), Arg.Any<CancellationToken>());
        deletedDocument.DeletedAt.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_ThrowsNotFoundException_WhenDocumentDoesNotExist()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        _repository.GetByIdIncludingDeletedAsync(documentId, Arg.Any<CancellationToken>()).Returns((Document?)null);

        var command = new RestoreDocument(DocumentId: documentId);

        // Act
        var act = () => _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{documentId}*");
    }

    [Fact]
    public async Task HandleAsync_ThrowsDomainException_WhenDocumentIsNotDeleted()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var activeDocument = new Document
        {
            Id = documentId,
            ClientId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            FileName = "test.pdf",
            GoogleDriveFileId = "drive-id",
            UploadedById = Guid.NewGuid(),
            CurrentVersion = 1,
            FileSize = 1024,
            MimeType = "application/pdf",
            TenantId = Guid.NewGuid(),
            DeletedAt = null
        };

        _repository.GetByIdIncludingDeletedAsync(documentId, Arg.Any<CancellationToken>()).Returns(activeDocument);

        var command = new RestoreDocument(DocumentId: documentId);

        // Act
        var act = () => _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorCode == "document_not_deleted");
    }
}
