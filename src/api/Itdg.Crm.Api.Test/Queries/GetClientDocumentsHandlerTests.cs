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

public class GetClientDocumentsHandlerTests
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IUserRepository _userRepository;
    private readonly IClientAssignmentRepository _clientAssignmentRepository;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ILogger<GetClientDocumentsHandler> _logger;
    private readonly GetClientDocumentsHandler _handler;
    private readonly Guid _clientId = Guid.NewGuid();
    private readonly Guid _categoryId = Guid.NewGuid();

    public GetClientDocumentsHandlerTests()
    {
        _documentRepository = Substitute.For<IDocumentRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _clientAssignmentRepository = Substitute.For<IClientAssignmentRepository>();
        _currentUserProvider = Substitute.For<ICurrentUserProvider>();
        _logger = Substitute.For<ILogger<GetClientDocumentsHandler>>();

        _handler = new GetClientDocumentsHandler(
            _documentRepository, _userRepository, _clientAssignmentRepository,
            _currentUserProvider, _logger);

        // Default: Administrator (sees all, no assignment check)
        _currentUserProvider.IsInRole(nameof(UserRole.Administrator)).Returns(true);
    }

    [Fact]
    public async Task HandleAsync_ReturnsPaginatedResult_WithCorrectData()
    {
        // Arrange
        var category = new DocumentCategory { Id = _categoryId, Name = "Tax Documents", SortOrder = 1, TenantId = Guid.NewGuid() };
        var documents = new List<Document>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ClientId = _clientId,
                CategoryId = _categoryId,
                Category = category,
                FileName = "tax-return.pdf",
                GoogleDriveFileId = "drive-file-1",
                UploadedById = Guid.NewGuid(),
                CurrentVersion = 1,
                FileSize = 1024,
                MimeType = "application/pdf",
                TenantId = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                ClientId = _clientId,
                CategoryId = _categoryId,
                Category = category,
                FileName = "quarterly-report.xlsx",
                GoogleDriveFileId = "drive-file-2",
                UploadedById = Guid.NewGuid(),
                CurrentVersion = 2,
                FileSize = 2048,
                MimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                TenantId = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            }
        };

        _documentRepository.GetPagedByClientIdAsync(
            _clientId, 1, 20, null, null, null, Arg.Any<CancellationToken>())
            .Returns((documents.AsReadOnly(), 2));

        var query = new GetClientDocuments(_clientId);

        // Act
        var result = await _handler.HandleAsync(query, Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task HandleAsync_MapsDocumentProperties_Correctly()
    {
        // Arrange
        var category = new DocumentCategory { Id = _categoryId, Name = "Contracts", SortOrder = 1, TenantId = Guid.NewGuid() };
        var documentId = Guid.NewGuid();
        var uploadedById = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow.AddDays(-1);
        var updatedAt = DateTimeOffset.UtcNow;

        var document = new Document
        {
            Id = documentId,
            ClientId = _clientId,
            CategoryId = _categoryId,
            Category = category,
            FileName = "contract.pdf",
            GoogleDriveFileId = "drive-file-123",
            UploadedById = uploadedById,
            CurrentVersion = 3,
            FileSize = 5120,
            MimeType = "application/pdf",
            TenantId = Guid.NewGuid(),
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        _documentRepository.GetPagedByClientIdAsync(
            _clientId, 1, 20, null, null, null, Arg.Any<CancellationToken>())
            .Returns((new List<Document> { document }.AsReadOnly(), 1));

        // Act
        var result = await _handler.HandleAsync(new GetClientDocuments(_clientId), Guid.NewGuid(), CancellationToken.None);

        // Assert
        var dto = result.Items.First();
        dto.DocumentId.Should().Be(documentId);
        dto.ClientId.Should().Be(_clientId);
        dto.CategoryId.Should().Be(_categoryId);
        dto.CategoryName.Should().Be("Contracts");
        dto.FileName.Should().Be("contract.pdf");
        dto.GoogleDriveFileId.Should().Be("drive-file-123");
        dto.UploadedById.Should().Be(uploadedById);
        dto.CurrentVersion.Should().Be(3);
        dto.FileSize.Should().Be(5120);
        dto.MimeType.Should().Be("application/pdf");
        dto.CreatedAt.Should().Be(createdAt);
        dto.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public async Task HandleAsync_PassesFilters_ToRepository()
    {
        // Arrange
        _documentRepository.GetPagedByClientIdAsync(
            _clientId, 2, 10, _categoryId, 2025, "tax", Arg.Any<CancellationToken>())
            .Returns((new List<Document>().AsReadOnly(), 0));

        var query = new GetClientDocuments(_clientId, Page: 2, PageSize: 10, CategoryId: _categoryId, Year: 2025, Search: "tax");

        // Act
        var result = await _handler.HandleAsync(query, Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _documentRepository.Received(1).GetPagedByClientIdAsync(
            _clientId, 2, 10, _categoryId, 2025, "tax", Arg.Any<CancellationToken>());
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_ReturnsEmptyResult_WhenNoDocuments()
    {
        // Arrange
        _documentRepository.GetPagedByClientIdAsync(
            _clientId, 1, 20, null, null, null, Arg.Any<CancellationToken>())
            .Returns((new List<Document>().AsReadOnly(), 0));

        // Act
        var result = await _handler.HandleAsync(new GetClientDocuments(_clientId), Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_Administrator_SkipsAssignmentCheck()
    {
        // Arrange
        _currentUserProvider.IsInRole(nameof(UserRole.Administrator)).Returns(true);

        _documentRepository.GetPagedByClientIdAsync(
            _clientId, 1, 20, null, null, null, Arg.Any<CancellationToken>())
            .Returns((new List<Document>().AsReadOnly(), 0));

        // Act
        await _handler.HandleAsync(new GetClientDocuments(_clientId), Guid.NewGuid(), CancellationToken.None);

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

        _documentRepository.GetPagedByClientIdAsync(
            _clientId, 1, 20, null, null, null, Arg.Any<CancellationToken>())
            .Returns((new List<Document>().AsReadOnly(), 0));

        // Act
        var result = await _handler.HandleAsync(new GetClientDocuments(_clientId), Guid.NewGuid(), CancellationToken.None);

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
        var act = () => _handler.HandleAsync(new GetClientDocuments(_clientId), Guid.NewGuid(), CancellationToken.None);

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
        var act = () => _handler.HandleAsync(new GetClientDocuments(_clientId), Guid.NewGuid(), CancellationToken.None);

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
        var act = () => _handler.HandleAsync(new GetClientDocuments(_clientId), Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
