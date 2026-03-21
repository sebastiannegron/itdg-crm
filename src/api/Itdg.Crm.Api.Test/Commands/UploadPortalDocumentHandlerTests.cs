namespace Itdg.Crm.Api.Test.Commands;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.CommandHandlers;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class UploadPortalDocumentHandlerTests
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IGenericRepository<DocumentVersion> _versionRepository;
    private readonly IGenericRepository<DocumentCategory> _categoryRepository;
    private readonly IGenericRepository<Client> _clientRepository;
    private readonly IGoogleDriveService _driveService;
    private readonly IGoogleDriveTokenProvider _tokenProvider;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<UploadPortalDocumentHandler> _logger;
    private readonly UploadPortalDocumentHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _clientId = Guid.NewGuid();
    private readonly Guid _categoryId = Guid.NewGuid();

    public UploadPortalDocumentHandlerTests()
    {
        _documentRepository = Substitute.For<IDocumentRepository>();
        _versionRepository = Substitute.For<IGenericRepository<DocumentVersion>>();
        _categoryRepository = Substitute.For<IGenericRepository<DocumentCategory>>();
        _clientRepository = Substitute.For<IGenericRepository<Client>>();
        _driveService = Substitute.For<IGoogleDriveService>();
        _tokenProvider = Substitute.For<IGoogleDriveTokenProvider>();
        _tenantProvider = Substitute.For<ITenantProvider>();
        _logger = Substitute.For<ILogger<UploadPortalDocumentHandler>>();

        _tenantProvider.GetTenantId().Returns(_tenantId);
        _tokenProvider.GetAccessToken().Returns("test-access-token");

        _clientRepository.GetByIdAsync(_clientId, Arg.Any<CancellationToken>())
            .Returns(new Client { Id = _clientId, Name = "Test Client", TenantId = _tenantId });

        _categoryRepository.GetByIdAsync(_categoryId, Arg.Any<CancellationToken>())
            .Returns(new DocumentCategory { Id = _categoryId, Name = "Tax Documents", NamingConvention = null, SortOrder = 1, TenantId = _tenantId });

        _driveService.UploadFileAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new DriveFileDto("drive-file-123", "test.pdf", "application/pdf", 1024, null, null, null, []));

        _handler = new UploadPortalDocumentHandler(
            _documentRepository, _versionRepository, _categoryRepository, _clientRepository,
            _driveService, _tokenProvider, _tenantProvider, _logger);
    }

    private UploadPortalDocument CreateValidCommand(Stream? stream = null)
    {
        return new UploadPortalDocument(
            ClientId: _clientId,
            CategoryId: _categoryId,
            FileName: "test-document.pdf",
            ContentStream: stream ?? new MemoryStream([1, 2, 3]),
            ContentType: "application/pdf",
            FileSize: 1024
        );
    }

    [Fact]
    public async Task HandleAsync_CreatesDocumentAndVersion_WithClientAsUploader()
    {
        // Arrange
        var command = CreateValidCommand();

        Document? capturedDocument = null;
        await _documentRepository.AddAsync(Arg.Do<Document>(d => capturedDocument = d), Arg.Any<CancellationToken>());

        DocumentVersion? capturedVersion = null;
        await _versionRepository.AddAsync(Arg.Do<DocumentVersion>(v => capturedVersion = v), Arg.Any<CancellationToken>());

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _documentRepository.Received(1).AddAsync(Arg.Any<Document>(), Arg.Any<CancellationToken>());
        await _versionRepository.Received(1).AddAsync(Arg.Any<DocumentVersion>(), Arg.Any<CancellationToken>());

        capturedDocument.Should().NotBeNull();
        capturedDocument!.ClientId.Should().Be(_clientId);
        capturedDocument.CategoryId.Should().Be(_categoryId);
        capturedDocument.FileName.Should().Be("test-document.pdf");
        capturedDocument.GoogleDriveFileId.Should().Be("drive-file-123");
        capturedDocument.UploadedById.Should().Be(_clientId);
        capturedDocument.CurrentVersion.Should().Be(1);
        capturedDocument.FileSize.Should().Be(1024);
        capturedDocument.MimeType.Should().Be("application/pdf");
        capturedDocument.TenantId.Should().Be(_tenantId);

        capturedVersion.Should().NotBeNull();
        capturedVersion!.DocumentId.Should().Be(capturedDocument.Id);
        capturedVersion.VersionNumber.Should().Be(1);
        capturedVersion.GoogleDriveFileId.Should().Be("drive-file-123");
        capturedVersion.UploadedById.Should().Be(_clientId);
    }

    [Fact]
    public async Task HandleAsync_UploadsToGoogleDrive_WithNullParentFolder()
    {
        // Arrange
        using var stream = new MemoryStream([1, 2, 3]);
        var command = CreateValidCommand(stream);

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _driveService.Received(1).UploadFileAsync(
            "test-access-token",
            "test-document.pdf",
            stream,
            "application/pdf",
            null,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ThrowsDomainException_WhenFileTypeNotAllowed()
    {
        // Arrange
        var command = new UploadPortalDocument(
            ClientId: _clientId,
            CategoryId: _categoryId,
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
        var command = new UploadPortalDocument(
            ClientId: _clientId,
            CategoryId: _categoryId,
            FileName: "large-file.pdf",
            ContentStream: new MemoryStream([1, 2, 3]),
            ContentType: "application/pdf",
            FileSize: 26 * 1024 * 1024
        );

        // Act
        var act = () => _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .Where(ex => ex.ErrorCode == "invalid_file_size");
    }

    [Fact]
    public async Task HandleAsync_ThrowsDomainException_WhenFileSizeIsZero()
    {
        // Arrange
        var command = new UploadPortalDocument(
            ClientId: _clientId,
            CategoryId: _categoryId,
            FileName: "empty.pdf",
            ContentStream: new MemoryStream(),
            ContentType: "application/pdf",
            FileSize: 0
        );

        // Act
        var act = () => _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .Where(ex => ex.ErrorCode == "invalid_file_size");
    }

    [Fact]
    public async Task HandleAsync_ThrowsNotFoundException_WhenClientNotFound()
    {
        // Arrange
        var unknownClientId = Guid.NewGuid();
        _clientRepository.GetByIdAsync(unknownClientId, Arg.Any<CancellationToken>()).Returns((Client?)null);

        var command = new UploadPortalDocument(
            ClientId: unknownClientId,
            CategoryId: _categoryId,
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
    public async Task HandleAsync_ThrowsNotFoundException_WhenCategoryNotFound()
    {
        // Arrange
        var unknownCategoryId = Guid.NewGuid();
        _categoryRepository.GetByIdAsync(unknownCategoryId, Arg.Any<CancellationToken>()).Returns((DocumentCategory?)null);

        var command = new UploadPortalDocument(
            ClientId: _clientId,
            CategoryId: unknownCategoryId,
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
    public async Task HandleAsync_EnforcesNamingConvention_WhenCategoryHasConvention()
    {
        // Arrange
        _categoryRepository.GetByIdAsync(_categoryId, Arg.Any<CancellationToken>())
            .Returns(new DocumentCategory
            {
                Id = _categoryId,
                Name = "Tax Documents",
                NamingConvention = "{ClientName}_TaxDoc_{Date}",
                SortOrder = 1,
                TenantId = _tenantId
            });

        var command = CreateValidCommand();

        Document? capturedDocument = null;
        await _documentRepository.AddAsync(Arg.Do<Document>(d => capturedDocument = d), Arg.Any<CancellationToken>());

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        capturedDocument.Should().NotBeNull();
        capturedDocument!.FileName.Should().StartWith("Test Client_TaxDoc_");
        capturedDocument.FileName.Should().EndWith(".pdf");
    }

    [Theory]
    [InlineData("application/pdf")]
    [InlineData("application/msword")]
    [InlineData("application/vnd.openxmlformats-officedocument.wordprocessingml.document")]
    [InlineData("application/vnd.ms-excel")]
    [InlineData("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    [InlineData("image/jpeg")]
    [InlineData("image/png")]
    [InlineData("text/plain")]
    [InlineData("text/csv")]
    public async Task HandleAsync_AcceptsAllowedFileTypes(string mimeType)
    {
        // Arrange
        var command = new UploadPortalDocument(
            ClientId: _clientId,
            CategoryId: _categoryId,
            FileName: "test-file.dat",
            ContentStream: new MemoryStream([1, 2, 3]),
            ContentType: mimeType,
            FileSize: 1024
        );

        // Act
        var act = () => _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync<DomainException>();
    }
}
