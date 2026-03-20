namespace Itdg.Crm.Api.Test.Commands;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.CommandHandlers;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class CreateClientHandlerTests
{
    private readonly IClientRepository _repository;
    private readonly ITenantProvider _tenantProvider;
    private readonly IGoogleDriveService _driveService;
    private readonly IGoogleDriveTokenProvider _tokenProvider;
    private readonly IGenericRepository<DocumentCategory> _categoryRepository;
    private readonly ILogger<CreateClientHandler> _logger;
    private readonly CreateClientHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    public CreateClientHandlerTests()
    {
        _repository = Substitute.For<IClientRepository>();
        _tenantProvider = Substitute.For<ITenantProvider>();
        _driveService = Substitute.For<IGoogleDriveService>();
        _tokenProvider = Substitute.For<IGoogleDriveTokenProvider>();
        _categoryRepository = Substitute.For<IGenericRepository<DocumentCategory>>();
        _logger = Substitute.For<ILogger<CreateClientHandler>>();
        _tenantProvider.GetTenantId().Returns(_tenantId);
        _handler = new CreateClientHandler(_repository, _tenantProvider, _driveService, _tokenProvider, _categoryRepository, _logger);
    }

    [Fact]
    public async Task HandleAsync_CreatesClient_WithCorrectProperties()
    {
        // Arrange
        var command = new CreateClient(
            Name: "Test Client",
            ContactEmail: "test@example.com",
            Phone: "787-555-1234",
            Address: "123 Main St",
            TierId: Guid.NewGuid(),
            Status: ClientStatus.Active,
            IndustryTag: "Technology",
            Notes: "Test notes",
            CustomFields: "{\"key\":\"value\"}"
        );

        Client? capturedClient = null;
        await _repository.AddAsync(Arg.Do<Client>(c => capturedClient = c), Arg.Any<CancellationToken>());

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _repository.Received(1).AddAsync(Arg.Any<Client>(), Arg.Any<CancellationToken>());
        capturedClient.Should().NotBeNull();
        capturedClient!.Name.Should().Be("Test Client");
        capturedClient.ContactEmail.Should().Be("test@example.com");
        capturedClient.Phone.Should().Be("787-555-1234");
        capturedClient.Address.Should().Be("123 Main St");
        capturedClient.TierId.Should().Be(command.TierId);
        capturedClient.Status.Should().Be(ClientStatus.Active);
        capturedClient.IndustryTag.Should().Be("Technology");
        capturedClient.Notes.Should().Be("Test notes");
        capturedClient.CustomFields.Should().Be("{\"key\":\"value\"}");
        capturedClient.TenantId.Should().Be(_tenantId);
        capturedClient.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task HandleAsync_SetsTenantId_FromTenantProvider()
    {
        // Arrange
        var command = new CreateClient(
            Name: "Test Client",
            ContactEmail: null,
            Phone: null,
            Address: null,
            TierId: null,
            Status: ClientStatus.Active,
            IndustryTag: null,
            Notes: null,
            CustomFields: null
        );

        Client? capturedClient = null;
        await _repository.AddAsync(Arg.Do<Client>(c => capturedClient = c), Arg.Any<CancellationToken>());

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        capturedClient.Should().NotBeNull();
        capturedClient!.TenantId.Should().Be(_tenantId);
    }

    [Fact]
    public async Task HandleAsync_CreatesDriveFolderStructure_WhenTokenAvailable()
    {
        // Arrange
        var command = new CreateClient(
            Name: "Acme Corp",
            ContactEmail: null,
            Phone: null,
            Address: null,
            TierId: null,
            Status: ClientStatus.Active,
            IndustryTag: null,
            Notes: null,
            CustomFields: null
        );

        _tokenProvider.GetAccessToken().Returns("test-access-token");

        var clientFolder = new DriveFileDto("folder-1", "Acme Corp", "application/vnd.google-apps.folder", null, null, null, null, []);
        var yearFolder = new DriveFileDto("folder-2", DateTimeOffset.UtcNow.Year.ToString(), "application/vnd.google-apps.folder", null, null, null, null, []);

        _driveService.CreateFolderAsync("test-access-token", "Acme Corp", null, Arg.Any<CancellationToken>())
            .Returns(clientFolder);
        _driveService.CreateFolderAsync("test-access-token", DateTimeOffset.UtcNow.Year.ToString(), "folder-1", Arg.Any<CancellationToken>())
            .Returns(yearFolder);

        var categories = new List<DocumentCategory>
        {
            new() { Id = Guid.NewGuid(), Name = "Tax Returns", NamingConvention = "tax-returns", SortOrder = 1, TenantId = _tenantId },
            new() { Id = Guid.NewGuid(), Name = "Financial Statements", NamingConvention = null, SortOrder = 2, TenantId = _tenantId }
        };
        _categoryRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(categories.AsReadOnly());

        _driveService.CreateFolderAsync("test-access-token", Arg.Any<string>(), "folder-2", Arg.Any<CancellationToken>())
            .Returns(new DriveFileDto("folder-3", "cat", "application/vnd.google-apps.folder", null, null, null, null, []));

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _driveService.Received(1).CreateFolderAsync("test-access-token", "Acme Corp", null, Arg.Any<CancellationToken>());
        await _driveService.Received(1).CreateFolderAsync("test-access-token", DateTimeOffset.UtcNow.Year.ToString(), "folder-1", Arg.Any<CancellationToken>());
        await _driveService.Received(1).CreateFolderAsync("test-access-token", "tax-returns", "folder-2", Arg.Any<CancellationToken>());
        await _driveService.Received(1).CreateFolderAsync("test-access-token", "Financial Statements", "folder-2", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_SkipsFolderCreation_WhenNoTokenAvailable()
    {
        // Arrange
        var command = new CreateClient(
            Name: "Test Client",
            ContactEmail: null,
            Phone: null,
            Address: null,
            TierId: null,
            Status: ClientStatus.Active,
            IndustryTag: null,
            Notes: null,
            CustomFields: null
        );

        _tokenProvider.GetAccessToken().Returns((string?)null);

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _repository.Received(1).AddAsync(Arg.Any<Client>(), Arg.Any<CancellationToken>());
        await _driveService.DidNotReceive().CreateFolderAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_CreatesClientSuccessfully_WhenDriveApiFails()
    {
        // Arrange
        var command = new CreateClient(
            Name: "Test Client",
            ContactEmail: null,
            Phone: null,
            Address: null,
            TierId: null,
            Status: ClientStatus.Active,
            IndustryTag: null,
            Notes: null,
            CustomFields: null
        );

        _tokenProvider.GetAccessToken().Returns("test-access-token");
        _driveService.CreateFolderAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns<DriveFileDto>(_ => throw new HttpRequestException("Google Drive API error"));

        Client? capturedClient = null;
        await _repository.AddAsync(Arg.Do<Client>(c => capturedClient = c), Arg.Any<CancellationToken>());

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        capturedClient.Should().NotBeNull();
        capturedClient!.Name.Should().Be("Test Client");
        await _repository.Received(1).AddAsync(Arg.Any<Client>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_CreatesFolderStructure_WithNoCategories()
    {
        // Arrange
        var command = new CreateClient(
            Name: "Empty Corp",
            ContactEmail: null,
            Phone: null,
            Address: null,
            TierId: null,
            Status: ClientStatus.Active,
            IndustryTag: null,
            Notes: null,
            CustomFields: null
        );

        _tokenProvider.GetAccessToken().Returns("test-access-token");

        var clientFolder = new DriveFileDto("folder-1", "Empty Corp", "application/vnd.google-apps.folder", null, null, null, null, []);
        var yearFolder = new DriveFileDto("folder-2", DateTimeOffset.UtcNow.Year.ToString(), "application/vnd.google-apps.folder", null, null, null, null, []);

        _driveService.CreateFolderAsync("test-access-token", "Empty Corp", null, Arg.Any<CancellationToken>())
            .Returns(clientFolder);
        _driveService.CreateFolderAsync("test-access-token", DateTimeOffset.UtcNow.Year.ToString(), "folder-1", Arg.Any<CancellationToken>())
            .Returns(yearFolder);

        _categoryRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<DocumentCategory>().AsReadOnly());

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _driveService.Received(1).CreateFolderAsync("test-access-token", "Empty Corp", null, Arg.Any<CancellationToken>());
        await _driveService.Received(1).CreateFolderAsync("test-access-token", DateTimeOffset.UtcNow.Year.ToString(), "folder-1", Arg.Any<CancellationToken>());
        // No category folder calls beyond the client and year folders
        await _driveService.Received(2).CreateFolderAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_UsesNamingConvention_WhenAvailable()
    {
        // Arrange
        var command = new CreateClient(
            Name: "Convention Corp",
            ContactEmail: null,
            Phone: null,
            Address: null,
            TierId: null,
            Status: ClientStatus.Active,
            IndustryTag: null,
            Notes: null,
            CustomFields: null
        );

        _tokenProvider.GetAccessToken().Returns("test-access-token");

        var clientFolder = new DriveFileDto("folder-1", "Convention Corp", "application/vnd.google-apps.folder", null, null, null, null, []);
        var yearFolder = new DriveFileDto("folder-2", DateTimeOffset.UtcNow.Year.ToString(), "application/vnd.google-apps.folder", null, null, null, null, []);

        _driveService.CreateFolderAsync("test-access-token", "Convention Corp", null, Arg.Any<CancellationToken>())
            .Returns(clientFolder);
        _driveService.CreateFolderAsync("test-access-token", DateTimeOffset.UtcNow.Year.ToString(), "folder-1", Arg.Any<CancellationToken>())
            .Returns(yearFolder);

        var categories = new List<DocumentCategory>
        {
            new() { Id = Guid.NewGuid(), Name = "Tax Returns", NamingConvention = "TAXRET", SortOrder = 1, TenantId = _tenantId },
            new() { Id = Guid.NewGuid(), Name = "Invoices", NamingConvention = "", SortOrder = 2, TenantId = _tenantId },
            new() { Id = Guid.NewGuid(), Name = "Contracts", NamingConvention = null, SortOrder = 3, TenantId = _tenantId }
        };
        _categoryRepository.GetAllAsync(Arg.Any<CancellationToken>()).Returns(categories.AsReadOnly());

        _driveService.CreateFolderAsync("test-access-token", Arg.Any<string>(), "folder-2", Arg.Any<CancellationToken>())
            .Returns(new DriveFileDto("folder-3", "cat", "application/vnd.google-apps.folder", null, null, null, null, []));

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert — NamingConvention used when non-empty, falls back to Name otherwise
        await _driveService.Received(1).CreateFolderAsync("test-access-token", "TAXRET", "folder-2", Arg.Any<CancellationToken>());
        await _driveService.Received(1).CreateFolderAsync("test-access-token", "Invoices", "folder-2", Arg.Any<CancellationToken>());
        await _driveService.Received(1).CreateFolderAsync("test-access-token", "Contracts", "folder-2", Arg.Any<CancellationToken>());
    }
}
