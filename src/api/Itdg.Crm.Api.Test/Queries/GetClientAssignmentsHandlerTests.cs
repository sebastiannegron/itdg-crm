namespace Itdg.Crm.Api.Test.Queries;

using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Application.QueryHandlers;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetClientAssignmentsHandlerTests
{
    private readonly IClientAssignmentRepository _repository;
    private readonly ILogger<GetClientAssignmentsHandler> _logger;
    private readonly GetClientAssignmentsHandler _handler;

    public GetClientAssignmentsHandlerTests()
    {
        _repository = Substitute.For<IClientAssignmentRepository>();
        _logger = Substitute.For<ILogger<GetClientAssignmentsHandler>>();
        _handler = new GetClientAssignmentsHandler(_repository, _logger);
    }

    [Fact]
    public async Task HandleAsync_ReturnsAssignments_WithUserDetails()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var assignedAt = DateTimeOffset.UtcNow;

        var assignments = new List<ClientAssignment>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                UserId = userId,
                AssignedAt = assignedAt,
                TenantId = Guid.NewGuid(),
                User = new User
                {
                    Id = userId,
                    EntraObjectId = "entra-1",
                    Email = "associate@example.com",
                    DisplayName = "Test Associate",
                    Role = UserRole.Associate,
                    TenantId = Guid.NewGuid()
                }
            }
        };

        _repository.GetByClientIdWithUserAsync(clientId, Arg.Any<CancellationToken>())
            .Returns(assignments.AsReadOnly());

        // Act
        var result = await _handler.HandleAsync(new GetClientAssignments(clientId), Guid.NewGuid(), CancellationToken.None);

        // Assert
        var dtos = result.ToList();
        dtos.Should().HaveCount(1);
        dtos[0].UserId.Should().Be(userId);
        dtos[0].DisplayName.Should().Be("Test Associate");
        dtos[0].Email.Should().Be("associate@example.com");
        dtos[0].AssignedAt.Should().Be(assignedAt);
    }

    [Fact]
    public async Task HandleAsync_ReturnsEmpty_WhenNoAssignments()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        _repository.GetByClientIdWithUserAsync(clientId, Arg.Any<CancellationToken>())
            .Returns(new List<ClientAssignment>().AsReadOnly());

        // Act
        var result = await _handler.HandleAsync(new GetClientAssignments(clientId), Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_HandlesNullUser_Gracefully()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var assignments = new List<ClientAssignment>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ClientId = clientId,
                UserId = userId,
                AssignedAt = DateTimeOffset.UtcNow,
                TenantId = Guid.NewGuid(),
                User = null
            }
        };

        _repository.GetByClientIdWithUserAsync(clientId, Arg.Any<CancellationToken>())
            .Returns(assignments.AsReadOnly());

        // Act
        var result = await _handler.HandleAsync(new GetClientAssignments(clientId), Guid.NewGuid(), CancellationToken.None);

        // Assert
        var dtos = result.ToList();
        dtos.Should().HaveCount(1);
        dtos[0].DisplayName.Should().BeEmpty();
        dtos[0].Email.Should().BeEmpty();
    }
}
