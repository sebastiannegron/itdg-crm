namespace Itdg.Crm.Api.Test.Queries;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Application.QueryHandlers;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class DownloadDocumentHandlerTests
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IClientAssignmentRepository _clientAssignmentRepository;
    private readonly IGoogleDriveTokenProvider _tokenProvider;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly IAuditService _auditService;
    private readonly ILogger<DownloadDocumentHandler> _logger;
    private readonly DownloadDocumentHandler _handler;
    private readonly Guid _documentId = Guid.NewGuid();
    private readonly Guid _clientId = Guid.NewGuid();
    private readonly Guid _categoryId = Guid.NewGuid();

    public DownloadDocumentHandlerTests()
    {
        _documentRepository = Substitute.For<IDocumentRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _clientAssignmentRepository = Substitute.For<IClientAssignmentRepository>();
        _tokenProvider = Substitute.For<IGoogleDriveTokenProvider>();
        _currentUserProvider = Substitute.For<ICurrentUserProvider>();
        _auditService = Substitute.For<IAuditService>();
        _logger = Substitute.For<ILogger<DownloadDocumentHandler>>();

        _handler = new DownloadDocumentHandler(
            _documentRepository, _userRepository, _clientAssignmentRepository,
            _tokenProvider, _currentUserProvider, _auditService, _logger);

        // Default: Administrator
        _currentUserProvider.IsInRole(nameof(UserRole.Administrator)).Returns(true);
        _tokenProvider.GetAccessToken().Returns("test-access-token");

        // Default document
        var category = new DocumentCategory { Id = _categoryId, Name = "Tax Documents", SortOrder = 1, TenantId = Guid.NewGuid() };
        _documentRepository.GetByIdWithCategoryAsync(_documentId, Arg.Any<CancellationToken>())
            .Returns(new Document
            {
                Id = _documentId,
                ClientId = _clientId,
                CategoryId = _categoryId,
                Category = category,
                FileName = "tax-return.pdf",
                GoogleDriveFileId = "drive-file-123",
                UploadedById = Guid.NewGuid(),
                CurrentVersion = 1,
                FileSize = 1024,
                MimeType = "application/pdf",
                TenantId = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            });
    }

    [Fact]
    public async Task HandleAsync_ReturnsDocumentDownloadDto_WithCorrectProperties()
    {
        // Act
        var result = await _handler.HandleAsync(new DownloadDocument(_documentId), Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.DocumentId.Should().Be(_documentId);
        result.FileName.Should().Be("tax-return.pdf");
        result.MimeType.Should().Be("application/pdf");
        result.FileSize.Should().Be(1024);
        result.GoogleDriveFileId.Should().Be("drive-file-123");
        result.WebViewLink.Should().Be("https://drive.google.com/file/d/drive-file-123/view");
    }

    [Fact]
    public async Task HandleAsync_ThrowsNotFoundException_WhenDocumentNotFound()
    {
        // Arrange
        var unknownId = Guid.NewGuid();
        _documentRepository.GetByIdWithCategoryAsync(unknownId, Arg.Any<CancellationToken>()).Returns((Document?)null);

        // Act
        var act = () => _handler.HandleAsync(new DownloadDocument(unknownId), Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_ThrowsDomainException_WhenGoogleDriveTokenUnavailable()
    {
        // Arrange
        _tokenProvider.GetAccessToken().Returns((string?)null);

        // Act
        var act = () => _handler.HandleAsync(new DownloadDocument(_documentId), Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .Where(ex => ex.ErrorCode == "google_drive_token_unavailable");
    }

    [Fact]
    public async Task HandleAsync_Administrator_SkipsAssignmentCheck()
    {
        // Arrange
        _currentUserProvider.IsInRole(nameof(UserRole.Administrator)).Returns(true);

        // Act
        await _handler.HandleAsync(new DownloadDocument(_documentId), Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _clientAssignmentRepository.DidNotReceive().ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _userRepository.DidNotReceive().GetByEntraObjectIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_Associate_Succeeds_WhenAssignedToClient()
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
        _clientAssignmentRepository.ExistsAsync(userId, _clientId, Arg.Any<CancellationToken>()).Returns(true);

        // Act
        var result = await _handler.HandleAsync(new DownloadDocument(_documentId), Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        await _clientAssignmentRepository.Received(1).ExistsAsync(userId, _clientId, Arg.Any<CancellationToken>());
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
        var act = () => _handler.HandleAsync(new DownloadDocument(_documentId), Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task HandleAsync_Associate_ThrowsForbidden_WhenUserNotFound()
    {
        // Arrange
        _currentUserProvider.IsInRole(nameof(UserRole.Administrator)).Returns(false);
        _currentUserProvider.GetEntraObjectId().Returns("unknown-entra-id");
        _userRepository.GetByEntraObjectIdAsync("unknown-entra-id", Arg.Any<CancellationToken>()).Returns((User?)null);

        // Act
        var act = () => _handler.HandleAsync(new DownloadDocument(_documentId), Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task HandleAsync_Associate_ThrowsForbidden_WhenEntraObjectIdMissing()
    {
        // Arrange
        _currentUserProvider.IsInRole(nameof(UserRole.Administrator)).Returns(false);
        _currentUserProvider.GetEntraObjectId().Returns((string?)null);

        // Act
        var act = () => _handler.HandleAsync(new DownloadDocument(_documentId), Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
