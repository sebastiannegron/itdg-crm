namespace Itdg.Crm.Api.Test.Commands;

using Itdg.Crm.Api.Application.CommandHandlers;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class DeleteDocumentHandlerTests
{
    private readonly IDocumentRepository _repository;
    private readonly ILogger<DeleteDocumentHandler> _logger;
    private readonly DeleteDocumentHandler _handler;

    public DeleteDocumentHandlerTests()
    {
        _repository = Substitute.For<IDocumentRepository>();
        _logger = Substitute.For<ILogger<DeleteDocumentHandler>>();
        _handler = new DeleteDocumentHandler(_repository, _logger);
    }

    [Fact]
    public async Task HandleAsync_SoftDeletesDocument_WhenDocumentExists()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var existingDocument = new Document
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

        _repository.GetByIdAsync(documentId, Arg.Any<CancellationToken>()).Returns(existingDocument);

        var command = new DeleteDocument(DocumentId: documentId);

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _repository.Received(1).UpdateAsync(Arg.Is<Document>(d =>
            d.Id == documentId &&
            d.DeletedAt != null
        ), Arg.Any<CancellationToken>());
        existingDocument.DeletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task HandleAsync_ThrowsNotFoundException_WhenDocumentDoesNotExist()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        _repository.GetByIdAsync(documentId, Arg.Any<CancellationToken>()).Returns((Document?)null);

        var command = new DeleteDocument(DocumentId: documentId);

        // Act
        var act = () => _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{documentId}*");
    }

    [Fact]
    public async Task HandleAsync_UsesUpdateNotDelete_ForSoftDelete()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var existingDocument = new Document
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
            TenantId = Guid.NewGuid()
        };

        _repository.GetByIdAsync(documentId, Arg.Any<CancellationToken>()).Returns(existingDocument);

        var command = new DeleteDocument(DocumentId: documentId);

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _repository.Received(1).UpdateAsync(Arg.Any<Document>(), Arg.Any<CancellationToken>());
        await _repository.DidNotReceive().DeleteAsync(Arg.Any<Document>(), Arg.Any<CancellationToken>());
    }
}
