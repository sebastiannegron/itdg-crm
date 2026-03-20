namespace Itdg.Crm.Api.Test.Queries;

using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Application.QueryHandlers;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetUsersHandlerTests
{
    private readonly IUserRepository _repository;
    private readonly ILogger<GetUsersHandler> _logger;
    private readonly GetUsersHandler _handler;

    public GetUsersHandlerTests()
    {
        _repository = Substitute.For<IUserRepository>();
        _logger = Substitute.For<ILogger<GetUsersHandler>>();
        _handler = new GetUsersHandler(_repository, _logger);
    }

    [Fact]
    public async Task HandleAsync_ReturnsPaginatedUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new()
            {
                Id = Guid.NewGuid(),
                EntraObjectId = "entra-1",
                Email = "user1@example.com",
                DisplayName = "User One",
                Role = UserRole.Administrator,
                IsActive = true,
                TenantId = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                EntraObjectId = "entra-2",
                Email = "user2@example.com",
                DisplayName = "User Two",
                Role = UserRole.Associate,
                IsActive = true,
                TenantId = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-2),
                UpdatedAt = DateTimeOffset.UtcNow
            }
        };

        _repository.GetPagedAsync(1, 20, null, null, null, Arg.Any<CancellationToken>())
            .Returns((users.AsReadOnly(), 2));

        var query = new GetUsers();

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
    public async Task HandleAsync_MapsUserPropertiesCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow.AddDays(-1);
        var updatedAt = DateTimeOffset.UtcNow;

        var users = new List<User>
        {
            new()
            {
                Id = userId,
                EntraObjectId = "entra-1",
                Email = "admin@example.com",
                DisplayName = "Admin User",
                Role = UserRole.Administrator,
                IsActive = true,
                TenantId = Guid.NewGuid(),
                CreatedAt = createdAt,
                UpdatedAt = updatedAt
            }
        };

        _repository.GetPagedAsync(1, 20, null, null, null, Arg.Any<CancellationToken>())
            .Returns((users.AsReadOnly(), 1));

        var query = new GetUsers();

        // Act
        var result = await _handler.HandleAsync(query, Guid.NewGuid(), CancellationToken.None);

        // Assert
        var user = result.Items.First();
        user.UserId.Should().Be(userId);
        user.EntraObjectId.Should().Be("entra-1");
        user.Email.Should().Be("admin@example.com");
        user.DisplayName.Should().Be("Admin User");
        user.Role.Should().Be("Administrator");
        user.IsActive.Should().BeTrue();
        user.CreatedAt.Should().Be(createdAt);
        user.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public async Task HandleAsync_ReturnsEmptyList_WhenNoUsersExist()
    {
        // Arrange
        _repository.GetPagedAsync(1, 20, null, null, null, Arg.Any<CancellationToken>())
            .Returns((new List<User>().AsReadOnly(), 0));

        var query = new GetUsers();

        // Act
        var result = await _handler.HandleAsync(query, Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_PassesFilterParameters_ToRepository()
    {
        // Arrange
        _repository.GetPagedAsync(2, 10, UserRole.Associate, true, "test", Arg.Any<CancellationToken>())
            .Returns((new List<User>().AsReadOnly(), 0));

        var query = new GetUsers(Page: 2, PageSize: 10, Role: UserRole.Associate, IsActive: true, Search: "test");

        // Act
        await _handler.HandleAsync(query, Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _repository.Received(1).GetPagedAsync(2, 10, UserRole.Associate, true, "test", Arg.Any<CancellationToken>());
    }
}
