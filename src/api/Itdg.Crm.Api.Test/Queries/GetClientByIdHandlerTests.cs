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

public class GetClientByIdHandlerTests
{
    private readonly IClientRepository _repository;
    private readonly IClientAssignmentRepository _clientAssignmentRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ILogger<GetClientByIdHandler> _logger;
    private readonly GetClientByIdHandler _handler;

    public GetClientByIdHandlerTests()
    {
        _repository = Substitute.For<IClientRepository>();
        _clientAssignmentRepository = Substitute.For<IClientAssignmentRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _currentUserProvider = Substitute.For<ICurrentUserProvider>();
        _logger = Substitute.For<ILogger<GetClientByIdHandler>>();
        _handler = new GetClientByIdHandler(
            _repository,
            _clientAssignmentRepository,
            _userRepository,
            _currentUserProvider,
            _logger);

        // Default: Administrator (bypasses assignment check)
        _currentUserProvider.IsInRole(nameof(UserRole.Administrator)).Returns(true);
    }

    [Fact]
    public async Task HandleAsync_ReturnsClientDto_WhenClientExists()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var tier = new ClientTier { Id = Guid.NewGuid(), Name = "Premium", SortOrder = 1, TenantId = Guid.NewGuid() };
        var client = new Client
        {
            Id = clientId,
            Name = "Test Client",
            ContactEmail = "test@example.com",
            Phone = "787-555-1234",
            Address = "123 Main St",
            TierId = tier.Id,
            Tier = tier,
            Status = ClientStatus.Active,
            IndustryTag = "Technology",
            Notes = "Test notes",
            CustomFields = "{\"key\":\"value\"}",
            TenantId = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _repository.GetByIdWithTierAsync(clientId, Arg.Any<CancellationToken>()).Returns(client);

        var query = new GetClientById(clientId);

        // Act
        var result = await _handler.HandleAsync(query, Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ClientId.Should().Be(clientId);
        result.Name.Should().Be("Test Client");
        result.ContactEmail.Should().Be("test@example.com");
        result.Phone.Should().Be("787-555-1234");
        result.Address.Should().Be("123 Main St");
        result.TierId.Should().Be(tier.Id);
        result.TierName.Should().Be("Premium");
        result.Status.Should().Be("Active");
        result.IndustryTag.Should().Be("Technology");
        result.Notes.Should().Be("Test notes");
        result.CustomFields.Should().Be("{\"key\":\"value\"}");
        result.CreatedAt.Should().Be(client.CreatedAt);
        result.UpdatedAt.Should().Be(client.UpdatedAt);
    }

    [Fact]
    public async Task HandleAsync_ReturnsNullTierName_WhenClientHasNoTier()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var client = new Client
        {
            Id = clientId,
            Name = "Test Client",
            TierId = null,
            Tier = null,
            Status = ClientStatus.Active,
            TenantId = Guid.NewGuid()
        };

        _repository.GetByIdWithTierAsync(clientId, Arg.Any<CancellationToken>()).Returns(client);

        var query = new GetClientById(clientId);

        // Act
        var result = await _handler.HandleAsync(query, Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.TierId.Should().BeNull();
        result.TierName.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_ThrowsNotFoundException_WhenClientDoesNotExist()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        _repository.GetByIdWithTierAsync(clientId, Arg.Any<CancellationToken>()).Returns((Client?)null);

        var query = new GetClientById(clientId);

        // Act
        var act = () => _handler.HandleAsync(query, Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{clientId}*");
    }

    [Fact]
    public async Task HandleAsync_Administrator_ReturnsClient_WithoutCheckingAssignment()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var client = new Client
        {
            Id = clientId,
            Name = "Admin Visible Client",
            Status = ClientStatus.Active,
            TenantId = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _currentUserProvider.IsInRole(nameof(UserRole.Administrator)).Returns(true);
        _repository.GetByIdWithTierAsync(clientId, Arg.Any<CancellationToken>()).Returns(client);

        // Act
        var result = await _handler.HandleAsync(new GetClientById(clientId), Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.ClientId.Should().Be(clientId);
        await _clientAssignmentRepository.DidNotReceive().ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _userRepository.DidNotReceive().GetByEntraObjectIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_Associate_ReturnsClient_WhenAssigned()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var entraObjectId = "associate-entra-id";
        var userId = Guid.NewGuid();

        var client = new Client
        {
            Id = clientId,
            Name = "Assigned Client",
            Status = ClientStatus.Active,
            TenantId = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

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
        _clientAssignmentRepository.ExistsAsync(userId, clientId, Arg.Any<CancellationToken>()).Returns(true);
        _repository.GetByIdWithTierAsync(clientId, Arg.Any<CancellationToken>()).Returns(client);

        // Act
        var result = await _handler.HandleAsync(new GetClientById(clientId), Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.ClientId.Should().Be(clientId);
        await _clientAssignmentRepository.Received(1).ExistsAsync(userId, clientId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_Associate_ThrowsNotFoundException_WhenNotAssigned()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var entraObjectId = "associate-entra-id";
        var userId = Guid.NewGuid();

        var client = new Client
        {
            Id = clientId,
            Name = "Unassigned Client",
            Status = ClientStatus.Active,
            TenantId = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

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
        _clientAssignmentRepository.ExistsAsync(userId, clientId, Arg.Any<CancellationToken>()).Returns(false);
        _repository.GetByIdWithTierAsync(clientId, Arg.Any<CancellationToken>()).Returns(client);

        // Act
        var act = () => _handler.HandleAsync(new GetClientById(clientId), Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{clientId}*");
    }

    [Fact]
    public async Task HandleAsync_Associate_ThrowsNotFoundException_WhenUserNotFoundInDatabase()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var entraObjectId = "unknown-entra-id";

        var client = new Client
        {
            Id = clientId,
            Name = "Some Client",
            Status = ClientStatus.Active,
            TenantId = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _currentUserProvider.IsInRole(nameof(UserRole.Administrator)).Returns(false);
        _currentUserProvider.GetEntraObjectId().Returns(entraObjectId);
        _userRepository.GetByEntraObjectIdAsync(entraObjectId, Arg.Any<CancellationToken>()).Returns((User?)null);
        _repository.GetByIdWithTierAsync(clientId, Arg.Any<CancellationToken>()).Returns(client);

        // Act
        var act = () => _handler.HandleAsync(new GetClientById(clientId), Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{clientId}*");
        await _clientAssignmentRepository.DidNotReceive().ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }
}
