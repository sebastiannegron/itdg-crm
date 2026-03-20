namespace Itdg.Crm.Api.Test.Commands;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.CommandHandlers;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class UploadDocumentHandlerTests
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IGenericRepository<DocumentVersion> _versionRepository;
    private readonly IGenericRepository<DocumentCategory> _categoryRepository;
    private readonly IGenericRepository<Client> _clientRepository;
    private readonly IGoogleDriveService _driveService;
    private readonly IGoogleDriveTokenProvider _tokenProvider;
    private readonly ITenantProvider _tenantProvider;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ILogger<UploadDocumentHandler> _logger;
    private readonly UploadDocumentHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _clientId = Guid.NewGuid();
    private readonly Guid _categoryId = Guid.NewGuid();

    public UploadDocumentHandlerTests()
    {
        _documentRepository = Substitute.For<IDocumentRepository>();
        _versionRepository = Substitute.For<IGenericRepository<DocumentVersion>>();
        _categoryRepository = Substitute.For<IGenericRepository<DocumentCategory>>();
        _clientRepository = Substitute.For<IGenericRepository<Client>>();
        _driveService = Substitute.For<IGoogleDriveService>();
        _tokenProvider = Substitute.For<IGoogleDriveTokenProvider>();
        _tenantProvider = Substitute.For<ITenantProvider>();
        _currentUserProvider = Substitute.For<ICurrentUserProvider>();
        _logger = Substitute.For<ILogger<UploadDocumentHandler>>();

        _tenantProvider.GetTenantId().Returns(_tenantId);
        _currentUserProvider.GetEntraObjectId().Returns(_userId.ToString());
        _tokenProvider.GetAccessToken().Returns("test-access-token");

        _clientRepository.GetByIdAsync(_clientId, Arg.Any<CancellationToken>())
            .Returns(new Client { Id = _clientId, Name = "Test Client", TenantId = _tenantId });

        _categoryRepository.GetByIdAsync(_categoryId, Arg.Any<CancellationToken>())
            .Returns(new DocumentCategory { Id = _categoryId, Name = "Tax Documents", NamingConvention = null, SortOrder = 1, TenantId = _tenantId });

        _driveService.UploadFileAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(new DriveFileDto("drive-file-123", "test.pdf", "application/pdf", 1024, null, null, null, []));

        _handler = new UploadDocumentHandler(
            _documentRepository, _versionRepository, _categoryRepository, _clientRepository,
            _driveService, _tokenProvider, _tenantProvider, _currentUserProvider, _logger);
    }

    private UploadDocument CreateValidCommand(Stream? stream = null)
    {
        return new UploadDocument(
            ClientId: _clientId,
            CategoryId: _categoryId,
            FileName: "test-document.pdf",
            ContentStream: stream ?? new MemoryStream([1, 2, 3]),
            ContentType: "application/pdf",
            FileSize: 1024,
            GoogleDriveParentFolderId: "parent-folder-id"
        );
    }

    [Fact]
    public async Task HandleAsync_CreatesDocumentAndVersion_WithCorrectProperties()
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
        capturedDocument.UploadedById.Should().Be(_userId);
        capturedDocument.CurrentVersion.Should().Be(1);
        capturedDocument.FileSize.Should().Be(1024);
        capturedDocument.MimeType.Should().Be("application/pdf");
        capturedDocument.TenantId.Should().Be(_tenantId);
        capturedDocument.Id.Should().NotBeEmpty();

        capturedVersion.Should().NotBeNull();
        capturedVersion!.DocumentId.Should().Be(capturedDocument.Id);
        capturedVersion.VersionNumber.Should().Be(1);
        capturedVersion.GoogleDriveFileId.Should().Be("drive-file-123");
        capturedVersion.UploadedById.Should().Be(_userId);
        capturedVersion.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task HandleAsync_UploadsToGoogleDrive_WithCorrectParameters()
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
            "parent-folder-id",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ThrowsDomainException_WhenFileTypeNotAllowed()
    {
        // Arrange
        var command = new UploadDocument(
            ClientId: _clientId,
            CategoryId: _categoryId,
            FileName: "malware.exe",
            ContentStream: new MemoryStream([1, 2, 3]),
            ContentType: "application/x-msdownload",
            FileSize: 1024,
            GoogleDriveParentFolderId: null
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
        var command = new UploadDocument(
            ClientId: _clientId,
            CategoryId: _categoryId,
            FileName: "large-file.pdf",
            ContentStream: new MemoryStream([1, 2, 3]),
            ContentType: "application/pdf",
            FileSize: 26 * 1024 * 1024, // 26 MB — exceeds 25 MB limit
            GoogleDriveParentFolderId: null
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
        var command = new UploadDocument(
            ClientId: _clientId,
            CategoryId: _categoryId,
            FileName: "empty.pdf",
            ContentStream: new MemoryStream(),
            ContentType: "application/pdf",
            FileSize: 0,
            GoogleDriveParentFolderId: null
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

        var command = new UploadDocument(
            ClientId: unknownClientId,
            CategoryId: _categoryId,
            FileName: "test.pdf",
            ContentStream: new MemoryStream([1, 2, 3]),
            ContentType: "application/pdf",
            FileSize: 1024,
            GoogleDriveParentFolderId: null
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

        var command = new UploadDocument(
            ClientId: _clientId,
            CategoryId: unknownCategoryId,
            FileName: "test.pdf",
            ContentStream: new MemoryStream([1, 2, 3]),
            ContentType: "application/pdf",
            FileSize: 1024,
            GoogleDriveParentFolderId: null
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

    [Fact]
    public async Task HandleAsync_KeepsOriginalFileName_WhenNoCategoryConvention()
    {
        // Arrange
        var command = CreateValidCommand();

        Document? capturedDocument = null;
        await _documentRepository.AddAsync(Arg.Do<Document>(d => capturedDocument = d), Arg.Any<CancellationToken>());

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        capturedDocument.Should().NotBeNull();
        capturedDocument!.FileName.Should().Be("test-document.pdf");
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
        var command = new UploadDocument(
            ClientId: _clientId,
            CategoryId: _categoryId,
            FileName: "test-file.dat",
            ContentStream: new MemoryStream([1, 2, 3]),
            ContentType: mimeType,
            FileSize: 1024,
            GoogleDriveParentFolderId: null
        );

        // Act
        var act = () => _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync<DomainException>();
    }

    [Fact]
    public void EnforceNamingConvention_ReturnsOriginalName_WhenNoConvention()
    {
        // Arrange
        var category = new DocumentCategory { Id = _categoryId, Name = "General", NamingConvention = null, SortOrder = 1, TenantId = _tenantId };

        // Act
        var result = UploadDocumentHandler.EnforceNamingConvention("original-file.pdf", category, "Test Client");

        // Assert
        result.Should().Be("original-file.pdf");
    }

    [Fact]
    public void EnforceNamingConvention_ReturnsOriginalName_WhenConventionIsEmpty()
    {
        // Arrange
        var category = new DocumentCategory { Id = _categoryId, Name = "General", NamingConvention = "", SortOrder = 1, TenantId = _tenantId };

        // Act
        var result = UploadDocumentHandler.EnforceNamingConvention("original-file.pdf", category, "Test Client");

        // Assert
        result.Should().Be("original-file.pdf");
    }

    [Fact]
    public void EnforceNamingConvention_AppliesConvention_WithPlaceholders()
    {
        // Arrange
        var category = new DocumentCategory
        {
            Id = _categoryId,
            Name = "Tax Documents",
            NamingConvention = "{ClientName}_TaxDoc_{Date}",
            SortOrder = 1,
            TenantId = _tenantId
        };

        // Act
        var result = UploadDocumentHandler.EnforceNamingConvention("my-file.pdf", category, "Acme Corp");

        // Assert
        var expectedDate = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd");
        result.Should().Be($"Acme Corp_TaxDoc_{expectedDate}.pdf");
    }

    [Fact]
    public void EnforceNamingConvention_IncludesFileNamePlaceholder()
    {
        // Arrange
        var category = new DocumentCategory
        {
            Id = _categoryId,
            Name = "Reports",
            NamingConvention = "{ClientName}_{FileName}_{Date}",
            SortOrder = 1,
            TenantId = _tenantId
        };

        // Act
        var result = UploadDocumentHandler.EnforceNamingConvention("quarterly-report.xlsx", category, "Acme Corp");

        // Assert
        var expectedDate = DateTimeOffset.UtcNow.ToString("yyyy-MM-dd");
        result.Should().Be($"Acme Corp_quarterly-report_{expectedDate}.xlsx");
    }
}
