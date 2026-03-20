namespace Itdg.Crm.Api.Test.Commands;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.CommandHandlers;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class AssignClientHandlerTests
{
    private readonly IClientAssignmentRepository _repository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<AssignClientHandler> _logger;
    private readonly AssignClientHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    public AssignClientHandlerTests()
    {
        _repository = Substitute.For<IClientAssignmentRepository>();
        _tenantProvider = Substitute.For<ITenantProvider>();
        _logger = Substitute.For<ILogger<AssignClientHandler>>();
        _tenantProvider.GetTenantId().Returns(_tenantId);
        _handler = new AssignClientHandler(_repository, _tenantProvider, _logger);
    }

    [Fact]
    public async Task HandleAsync_CreatesAssignment_WithCorrectProperties()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new AssignClient(ClientId: clientId, UserId: userId);

        _repository.ExistsAsync(userId, clientId, Arg.Any<CancellationToken>()).Returns(false);

        ClientAssignment? capturedAssignment = null;
        await _repository.AddAsync(Arg.Do<ClientAssignment>(a => capturedAssignment = a), Arg.Any<CancellationToken>());

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _repository.Received(1).AddAsync(Arg.Any<ClientAssignment>(), Arg.Any<CancellationToken>());
        capturedAssignment.Should().NotBeNull();
        capturedAssignment!.ClientId.Should().Be(clientId);
        capturedAssignment.UserId.Should().Be(userId);
        capturedAssignment.TenantId.Should().Be(_tenantId);
        capturedAssignment.Id.Should().NotBeEmpty();
        capturedAssignment.AssignedAt.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task HandleAsync_ThrowsConflictException_WhenAssignmentAlreadyExists()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var command = new AssignClient(ClientId: clientId, UserId: userId);

        _repository.ExistsAsync(userId, clientId, Arg.Any<CancellationToken>()).Returns(true);

        // Act
        var act = () => _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage($"*{clientId}*");
        await _repository.DidNotReceive().AddAsync(Arg.Any<ClientAssignment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_SetsTenantId_FromTenantProvider()
    {
        // Arrange
        var command = new AssignClient(ClientId: Guid.NewGuid(), UserId: Guid.NewGuid());

        _repository.ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(false);

        ClientAssignment? capturedAssignment = null;
        await _repository.AddAsync(Arg.Do<ClientAssignment>(a => capturedAssignment = a), Arg.Any<CancellationToken>());

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        capturedAssignment.Should().NotBeNull();
        capturedAssignment!.TenantId.Should().Be(_tenantId);
    }
}
