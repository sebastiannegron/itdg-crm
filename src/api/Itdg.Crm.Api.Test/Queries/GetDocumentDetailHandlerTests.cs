namespace Itdg.Crm.Api.Test.Queries;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Application.QueryHandlers;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetDocumentDetailHandlerTests
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IClientAssignmentRepository _clientAssignmentRepository;
    private readonly IGoogleDriveTokenProvider _tokenProvider;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ILogger<GetDocumentDetailHandler> _logger;
    private readonly GetDocumentDetailHandler _handler;
    private readonly Guid _documentId = Guid.NewGuid();
    private readonly Guid _clientId = Guid.NewGuid();
    private readonly Guid _categoryId = Guid.NewGuid();
    private readonly Guid _uploadedById = Guid.NewGuid();

    public GetDocumentDetailHandlerTests()
    {
        _documentRepository = Substitute.For<IDocumentRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _clientAssignmentRepository = Substitute.For<IClientAssignmentRepository>();
        _tokenProvider = Substitute.For<IGoogleDriveTokenProvider>();
        _currentUserProvider = Substitute.For<ICurrentUserProvider>();
        _logger = Substitute.For<ILogger<GetDocumentDetailHandler>>();

        _handler = new GetDocumentDetailHandler(
            _documentRepository, _userRepository, _clientAssignmentRepository,
            _tokenProvider, _currentUserProvider, _logger);

        // Default: Administrator
        _currentUserProvider.IsInRole(nameof(UserRole.Administrator)).Returns(true);
        _tokenProvider.GetAccessToken().Returns("test-access-token");

        // Default document with versions
        var category = new DocumentCategory { Id = _categoryId, Name = "Tax Documents", SortOrder = 1, TenantId = Guid.NewGuid() };
        var document = new Document
        {
            Id = _documentId,
            ClientId = _clientId,
            CategoryId = _categoryId,
            Category = category,
            FileName = "tax-return.pdf",
            GoogleDriveFileId = "drive-file-123",
            UploadedById = _uploadedById,
            CurrentVersion = 2,
            FileSize = 2048,
            MimeType = "application/pdf",
            TenantId = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var versions = new List<DocumentVersion>
        {
            new()
            {
                Id = Guid.NewGuid(),
                DocumentId = _documentId,
                VersionNumber = 1,
                GoogleDriveFileId = "drive-v1",
                UploadedById = _uploadedById,
                UploadedAt = DateTimeOffset.UtcNow.AddDays(-1)
            },
            new()
            {
                Id = Guid.NewGuid(),
                DocumentId = _documentId,
                VersionNumber = 2,
                GoogleDriveFileId = "drive-v2",
                UploadedById = _uploadedById,
                UploadedAt = DateTimeOffset.UtcNow
            }
        };

        _documentRepository.GetByIdWithVersionsAsync(_documentId, Arg.Any<CancellationToken>())
            .Returns((document, (IReadOnlyList<DocumentVersion>)versions));
    }

    [Fact]
    public async Task HandleAsync_ReturnsDocumentDetailDto_WithCorrectProperties()
    {
        // Act
        var result = await _handler.HandleAsync(new GetDocumentDetail(_documentId), Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.DocumentId.Should().Be(_documentId);
        result.ClientId.Should().Be(_clientId);
        result.CategoryId.Should().Be(_categoryId);
        result.CategoryName.Should().Be("Tax Documents");
        result.FileName.Should().Be("tax-return.pdf");
        result.GoogleDriveFileId.Should().Be("drive-file-123");
        result.UploadedById.Should().Be(_uploadedById);
        result.CurrentVersion.Should().Be(2);
        result.FileSize.Should().Be(2048);
        result.MimeType.Should().Be("application/pdf");
        result.WebViewLink.Should().Be("https://drive.google.com/file/d/drive-file-123/view");
        result.Versions.Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_ThrowsNotFoundException_WhenDocumentNotFound()
    {
        // Arrange
        var unknownId = Guid.NewGuid();
        _documentRepository.GetByIdWithVersionsAsync(unknownId, Arg.Any<CancellationToken>())
            .Returns(((Document?)null, (IReadOnlyList<DocumentVersion>)new List<DocumentVersion>()));

        // Act
        var act = () => _handler.HandleAsync(new GetDocumentDetail(unknownId), Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_Administrator_SkipsAssignmentCheck()
    {
        // Arrange
        _currentUserProvider.IsInRole(nameof(UserRole.Administrator)).Returns(true);

        // Act
        await _handler.HandleAsync(new GetDocumentDetail(_documentId), Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _clientAssignmentRepository.DidNotReceive().ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _userRepository.DidNotReceive().GetByEntraObjectIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_Associate_ThrowsForbidden_WhenNotAssignedToClient()
    {
        // Arrange
        var entraObjectId = "test-entra-id";
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            EntraObjectId = entraObjectId,
            Email = "associate@example.com",
            DisplayName = "Associate User",
            Role = UserRole.Associate,
            TenantId = Guid.NewGuid()
        };

        _currentUserProvider.IsInRole(nameof(UserRole.Administrator)).Returns(false);
        _currentUserProvider.GetEntraObjectId().Returns(entraObjectId);
        _userRepository.GetByEntraObjectIdAsync(entraObjectId, Arg.Any<CancellationToken>()).Returns(user);
        _clientAssignmentRepository.ExistsAsync(userId, _clientId, Arg.Any<CancellationToken>()).Returns(false);

        // Act
        var act = () => _handler.HandleAsync(new GetDocumentDetail(_documentId), Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task HandleAsync_ReturnsNullWebViewLink_WhenGoogleDriveTokenUnavailable()
    {
        // Arrange
        _tokenProvider.GetAccessToken().Returns((string?)null);

        // Act
        var result = await _handler.HandleAsync(new GetDocumentDetail(_documentId), Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.WebViewLink.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_ReturnsEmptyVersionsList_WhenNoVersionsExist()
    {
        // Arrange
        var document = new Document
        {
            Id = _documentId,
            ClientId = _clientId,
            CategoryId = _categoryId,
            Category = new DocumentCategory { Id = _categoryId, Name = "Tax Documents", SortOrder = 1, TenantId = Guid.NewGuid() },
            FileName = "tax-return.pdf",
            GoogleDriveFileId = "drive-file-123",
            UploadedById = _uploadedById,
            CurrentVersion = 1,
            FileSize = 1024,
            MimeType = "application/pdf",
            TenantId = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _documentRepository.GetByIdWithVersionsAsync(_documentId, Arg.Any<CancellationToken>())
            .Returns((document, (IReadOnlyList<DocumentVersion>)new List<DocumentVersion>()));

        // Act
        var result = await _handler.HandleAsync(new GetDocumentDetail(_documentId), Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Versions.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_ReturnsVersions_MappedCorrectly()
    {
        // Arrange
        var versionId1 = Guid.NewGuid();
        var versionId2 = Guid.NewGuid();
        var versionId3 = Guid.NewGuid();
        var uploadedAt1 = DateTimeOffset.UtcNow.AddDays(-2);
        var uploadedAt2 = DateTimeOffset.UtcNow.AddDays(-1);
        var uploadedAt3 = DateTimeOffset.UtcNow;

        var document = new Document
        {
            Id = _documentId,
            ClientId = _clientId,
            CategoryId = _categoryId,
            Category = new DocumentCategory { Id = _categoryId, Name = "Tax Documents", SortOrder = 1, TenantId = Guid.NewGuid() },
            FileName = "tax-return.pdf",
            GoogleDriveFileId = "drive-file-123",
            UploadedById = _uploadedById,
            CurrentVersion = 3,
            FileSize = 1024,
            MimeType = "application/pdf",
            TenantId = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var versions = new List<DocumentVersion>
        {
            new() { Id = versionId1, DocumentId = _documentId, VersionNumber = 3, GoogleDriveFileId = "drive-v3", UploadedById = _uploadedById, UploadedAt = uploadedAt3 },
            new() { Id = versionId2, DocumentId = _documentId, VersionNumber = 2, GoogleDriveFileId = "drive-v2", UploadedById = _uploadedById, UploadedAt = uploadedAt2 },
            new() { Id = versionId3, DocumentId = _documentId, VersionNumber = 1, GoogleDriveFileId = "drive-v1", UploadedById = _uploadedById, UploadedAt = uploadedAt1 }
        };

        _documentRepository.GetByIdWithVersionsAsync(_documentId, Arg.Any<CancellationToken>())
            .Returns((document, (IReadOnlyList<DocumentVersion>)versions));

        // Act
        var result = await _handler.HandleAsync(new GetDocumentDetail(_documentId), Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Versions.Should().HaveCount(3);
        result.Versions[0].VersionNumber.Should().Be(3);
        result.Versions[1].VersionNumber.Should().Be(2);
        result.Versions[2].VersionNumber.Should().Be(1);
        result.Versions[0].GoogleDriveFileId.Should().Be("drive-v3");
        result.Versions[0].DocumentId.Should().Be(_documentId);
        result.Versions[0].UploadedById.Should().Be(_uploadedById);
    }
}
