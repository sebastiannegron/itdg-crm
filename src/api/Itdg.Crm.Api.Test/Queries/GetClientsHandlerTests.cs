namespace Itdg.Crm.Api.Test.Queries;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Application.QueryHandlers;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetClientsHandlerTests
{
    private readonly IClientRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ILogger<GetClientsHandler> _logger;
    private readonly GetClientsHandler _handler;

    public GetClientsHandlerTests()
    {
        _repository = Substitute.For<IClientRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _currentUserProvider = Substitute.For<ICurrentUserProvider>();
        _logger = Substitute.For<ILogger<GetClientsHandler>>();
        _handler = new GetClientsHandler(_repository, _userRepository, _currentUserProvider, _logger);

        // Default: Administrator (sees all clients, no assignment filter)
        _currentUserProvider.IsInRole(nameof(UserRole.Administrator)).Returns(true);
    }

    [Fact]
    public async Task HandleAsync_ReturnsPaginatedResult_WithCorrectData()
    {
        // Arrange
        var tier = new ClientTier { Id = Guid.NewGuid(), Name = "Premium", SortOrder = 1, TenantId = Guid.NewGuid() };
        var clients = new List<Client>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Client A",
                ContactEmail = "a@example.com",
                TierId = tier.Id,
                Tier = tier,
                Status = ClientStatus.Active,
                TenantId = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Client B",
                Status = ClientStatus.Inactive,
                TenantId = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            }
        };

        _repository.GetPagedAsync(1, 20, null, null, null, null, Arg.Any<CancellationToken>())
            .Returns((clients.AsReadOnly(), 2));

        var query = new GetClients();

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
    public async Task HandleAsync_MapsClientProperties_Correctly()
    {
        // Arrange
        var tier = new ClientTier { Id = Guid.NewGuid(), Name = "Standard", SortOrder = 2, TenantId = Guid.NewGuid() };
        var client = new Client
        {
            Id = Guid.NewGuid(),
            Name = "Test Client",
            ContactEmail = "test@example.com",
            Phone = "787-555-1234",
            Address = "123 Main St",
            TierId = tier.Id,
            Tier = tier,
            Status = ClientStatus.Active,
            IndustryTag = "Technology",
            Notes = "Notes",
            CustomFields = "{\"key\":\"value\"}",
            TenantId = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _repository.GetPagedAsync(1, 20, null, null, null, null, Arg.Any<CancellationToken>())
            .Returns((new List<Client> { client }.AsReadOnly(), 1));

        var query = new GetClients();

        // Act
        var result = await _handler.HandleAsync(query, Guid.NewGuid(), CancellationToken.None);

        // Assert
        var dto = result.Items.First();
        dto.ClientId.Should().Be(client.Id);
        dto.Name.Should().Be("Test Client");
        dto.ContactEmail.Should().Be("test@example.com");
        dto.Phone.Should().Be("787-555-1234");
        dto.Address.Should().Be("123 Main St");
        dto.TierId.Should().Be(tier.Id);
        dto.TierName.Should().Be("Standard");
        dto.Status.Should().Be("Active");
        dto.IndustryTag.Should().Be("Technology");
        dto.Notes.Should().Be("Notes");
        dto.CustomFields.Should().Be("{\"key\":\"value\"}");
    }

    [Fact]
    public async Task HandleAsync_PassesFilters_ToRepository()
    {
        // Arrange
        var tierId = Guid.NewGuid();
        _repository.GetPagedAsync(2, 10, ClientStatus.Active, tierId, "search", null, Arg.Any<CancellationToken>())
            .Returns((new List<Client>().AsReadOnly(), 0));

        var query = new GetClients(Page: 2, PageSize: 10, Status: ClientStatus.Active, TierId: tierId, Search: "search");

        // Act
        var result = await _handler.HandleAsync(query, Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _repository.Received(1).GetPagedAsync(2, 10, ClientStatus.Active, tierId, "search", null, Arg.Any<CancellationToken>());
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_ReturnsEmptyResult_WhenNoClientsFound()
    {
        // Arrange
        _repository.GetPagedAsync(1, 20, null, null, null, null, Arg.Any<CancellationToken>())
            .Returns((new List<Client>().AsReadOnly(), 0));

        var query = new GetClients();

        // Act
        var result = await _handler.HandleAsync(query, Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_Administrator_ReturnsAllClients_WithoutAssignmentFilter()
    {
        // Arrange
        _currentUserProvider.IsInRole(nameof(UserRole.Administrator)).Returns(true);

        var clients = new List<Client>
        {
            new() { Id = Guid.NewGuid(), Name = "Client A", Status = ClientStatus.Active, TenantId = Guid.NewGuid(), CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow },
            new() { Id = Guid.NewGuid(), Name = "Client B", Status = ClientStatus.Active, TenantId = Guid.NewGuid(), CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow }
        };

        _repository.GetPagedAsync(1, 20, null, null, null, null, Arg.Any<CancellationToken>())
            .Returns((clients.AsReadOnly(), 2));

        // Act
        var result = await _handler.HandleAsync(new GetClients(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        await _repository.Received(1).GetPagedAsync(1, 20, null, null, null, null, Arg.Any<CancellationToken>());
        await _userRepository.DidNotReceive().GetByEntraObjectIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_Associate_ReturnsOnlyAssignedClients()
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

        var assignedClient = new Client
        {
            Id = Guid.NewGuid(),
            Name = "Assigned Client",
            Status = ClientStatus.Active,
            TenantId = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _repository.GetPagedAsync(1, 20, null, null, null, userId, Arg.Any<CancellationToken>())
            .Returns((new List<Client> { assignedClient }.AsReadOnly(), 1));

        // Act
        var result = await _handler.HandleAsync(new GetClients(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
        result.Items.First().Name.Should().Be("Assigned Client");
        await _repository.Received(1).GetPagedAsync(1, 20, null, null, null, userId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_Associate_ReturnsEmptyList_WhenNoAssignedClients()
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

        _repository.GetPagedAsync(1, 20, null, null, null, userId, Arg.Any<CancellationToken>())
            .Returns((new List<Client>().AsReadOnly(), 0));

        // Act
        var result = await _handler.HandleAsync(new GetClients(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_Associate_ReturnsEmptyList_WhenUserNotFoundInDatabase()
    {
        // Arrange
        var entraObjectId = "unknown-entra-id";

        _currentUserProvider.IsInRole(nameof(UserRole.Administrator)).Returns(false);
        _currentUserProvider.GetEntraObjectId().Returns(entraObjectId);
        _userRepository.GetByEntraObjectIdAsync(entraObjectId, Arg.Any<CancellationToken>()).Returns((User?)null);

        // Act
        var result = await _handler.HandleAsync(new GetClients(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        await _repository.DidNotReceive().GetPagedAsync(
            Arg.Any<int>(), Arg.Any<int>(), Arg.Any<ClientStatus?>(), Arg.Any<Guid?>(), Arg.Any<string?>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>());
    }
}
