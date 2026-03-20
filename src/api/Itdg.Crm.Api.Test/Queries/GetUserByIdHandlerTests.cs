namespace Itdg.Crm.Api.Test.Queries;

using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Application.QueryHandlers;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetUserByIdHandlerTests
{
    private readonly IUserRepository _repository;
    private readonly ILogger<GetUserByIdHandler> _logger;
    private readonly GetUserByIdHandler _handler;

    public GetUserByIdHandlerTests()
    {
        _repository = Substitute.For<IUserRepository>();
        _logger = Substitute.For<ILogger<GetUserByIdHandler>>();
        _handler = new GetUserByIdHandler(_repository, _logger);
    }

    [Fact]
    public async Task HandleAsync_ReturnsUserDto_WhenUserExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow.AddDays(-1);
        var updatedAt = DateTimeOffset.UtcNow;

        var user = new User
        {
            Id = userId,
            EntraObjectId = "entra-123",
            Email = "user@example.com",
            DisplayName = "Test User",
            Role = UserRole.Administrator,
            IsActive = true,
            TenantId = Guid.NewGuid(),
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        _repository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns(user);

        var query = new GetUserById(userId);

        // Act
        var result = await _handler.HandleAsync(query, Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.EntraObjectId.Should().Be("entra-123");
        result.Email.Should().Be("user@example.com");
        result.DisplayName.Should().Be("Test User");
        result.Role.Should().Be("Administrator");
        result.IsActive.Should().BeTrue();
        result.CreatedAt.Should().Be(createdAt);
        result.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public async Task HandleAsync_ThrowsNotFoundException_WhenUserDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _repository.GetByIdAsync(userId, Arg.Any<CancellationToken>()).Returns((User?)null);

        var query = new GetUserById(userId);

        // Act
        var act = () => _handler.HandleAsync(query, Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{userId}*");
    }
}
