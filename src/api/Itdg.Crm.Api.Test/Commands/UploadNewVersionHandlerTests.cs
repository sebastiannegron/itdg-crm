namespace Itdg.Crm.Api.Test.Commands;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.CommandHandlers;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class UploadNewVersionHandlerTests
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IGenericRepository<DocumentVersion> _versionRepository;
    private readonly IGoogleDriveService _driveService;
    private readonly IGoogleDriveTokenProvider _tokenProvider;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ILogger<UploadNewVersionHandler> _logger;
    private readonly UploadNewVersionHandler _handler;
    private readonly Guid _documentId = Guid.NewGuid();
    private readonly Guid _clientId = Guid.NewGuid();
    private readonly Guid _categoryId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();

    public UploadNewVersionHandlerTests()
    {
        _documentRepository = Substitute.For<IDocumentRepository>();
        _versionRepository = Substitute.For<IGenericRepository<DocumentVersion>>();
        _driveService = Substitute.For<IGoogleDriveService>();
        _tokenProvider = Substitute.For<IGoogleDriveTokenProvider>();
        _currentUserProvider = Substitute.For<ICurrentUserProvider>();
        _logger = Substitute.For<ILogger<UploadNewVersionHandler>>();

        _currentUserProvider.GetEntraObjectId().Returns(_userId.ToString());
        _tokenProvider.GetAccessToken().Returns("test-access-token");

        _documentRepository.GetByIdWithCategoryAsync(_documentId, Arg.Any<CancellationToken>())
            .Returns(new Document
            {
                Id = _documentId,
                ClientId = _clientId,
                CategoryId = _categoryId,
                Category = new DocumentCategory { Id = _categoryId, Name = "Tax Documents", SortOrder = 1, TenantId = Guid.NewGuid() },
                FileName = "tax-return.pdf",
                GoogleDriveFileId = "drive-file-old",
                UploadedById = Guid.NewGuid(),
                CurrentVersion = 2,
                FileSize = 1024,
                MimeType = "application/pdf",
                TenantId = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });

        _driveService.UploadFileAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new DriveFileDto("drive-file-new", "tax-return.pdf", "application/pdf", 2048, null, null, null, []));

        _handler = new UploadNewVersionHandler(
            _documentRepository, _versionRepository, _driveService,
            _tokenProvider, _currentUserProvider, _logger);
    }

    private UploadNewVersion CreateValidCommand(Stream? stream = null)
    {
        return new UploadNewVersion(
            DocumentId: _documentId,
            FileName: "tax-return-v3.pdf",
            ContentStream: stream ?? new MemoryStream([1, 2, 3]),
            ContentType: "application/pdf",
            FileSize: 2048
        );
    }

    [Fact]
    public async Task HandleAsync_IncrementsVersionAndCreatesDocumentVersion()
    {
        // Arrange
        var command = CreateValidCommand();

        Document? capturedDocument = null;
        await _documentRepository.UpdateAsync(Arg.Do<Document>(d => capturedDocument = d), Arg.Any<CancellationToken>());

        DocumentVersion? capturedVersion = null;
        await _versionRepository.AddAsync(Arg.Do<DocumentVersion>(v => capturedVersion = v), Arg.Any<CancellationToken>());

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _versionRepository.Received(1).AddAsync(Arg.Any<DocumentVersion>(), Arg.Any<CancellationToken>());
        await _documentRepository.Received(1).UpdateAsync(Arg.Any<Document>(), Arg.Any<CancellationToken>());

        capturedVersion.Should().NotBeNull();
        capturedVersion!.DocumentId.Should().Be(_documentId);
        capturedVersion.VersionNumber.Should().Be(3);
        capturedVersion.GoogleDriveFileId.Should().Be("drive-file-new");
        capturedVersion.UploadedById.Should().Be(_userId);
        capturedVersion.Id.Should().NotBeEmpty();

        capturedDocument.Should().NotBeNull();
        capturedDocument!.CurrentVersion.Should().Be(3);
    }

    [Fact]
    public async Task HandleAsync_ThrowsDomainException_WhenFileTypeNotAllowed()
    {
        // Arrange
        var command = new UploadNewVersion(
            DocumentId: _documentId,
            FileName: "malware.exe",
            ContentStream: new MemoryStream([1, 2, 3]),
            ContentType: "application/x-msdownload",
            FileSize: 1024
        );

        // Act
        var act = () => _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .Where(ex => ex.ErrorCode == "invalid_file_type");

        await _driveService.DidNotReceive().UploadFileAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ThrowsDomainException_WhenFileSizeExceedsLimit()
    {
        // Arrange
        var command = new UploadNewVersion(
            DocumentId: _documentId,
            FileName: "large-file.pdf",
            ContentStream: new MemoryStream([1, 2, 3]),
            ContentType: "application/pdf",
            FileSize: 26 * 1024 * 1024 // 26 MB — exceeds 25 MB limit
        );

        // Act
        var act = () => _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .Where(ex => ex.ErrorCode == "invalid_file_size");
    }

    [Fact]
    public async Task HandleAsync_ThrowsNotFoundException_WhenDocumentNotFound()
    {
        // Arrange
        var unknownId = Guid.NewGuid();
        _documentRepository.GetByIdWithCategoryAsync(unknownId, Arg.Any<CancellationToken>()).Returns((Document?)null);

        var command = new UploadNewVersion(
            DocumentId: unknownId,
            FileName: "test.pdf",
            ContentStream: new MemoryStream([1, 2, 3]),
            ContentType: "application/pdf",
            FileSize: 1024
        );

        // Act
        var act = () => _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_ThrowsDomainException_WhenGoogleDriveTokenUnavailable()
    {
        // Arrange
        _tokenProvider.GetAccessToken().Returns((string?)null);
        var command = CreateValidCommand();

        // Act
        var act = () => _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .Where(ex => ex.ErrorCode == "google_drive_token_unavailable");
    }

    [Fact]
    public async Task HandleAsync_UpdatesDocumentProperties_AfterUpload()
    {
        // Arrange
        var command = CreateValidCommand();

        Document? capturedDocument = null;
        await _documentRepository.UpdateAsync(Arg.Do<Document>(d => capturedDocument = d), Arg.Any<CancellationToken>());

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        capturedDocument.Should().NotBeNull();
        capturedDocument!.GoogleDriveFileId.Should().Be("drive-file-new");
        capturedDocument.FileSize.Should().Be(2048);
        capturedDocument.MimeType.Should().Be("application/pdf");
        capturedDocument.CurrentVersion.Should().Be(3);
    }
}
