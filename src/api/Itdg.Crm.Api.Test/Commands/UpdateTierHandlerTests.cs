namespace Itdg.Crm.Api.Test.Commands;

using Itdg.Crm.Api.Application.CommandHandlers;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class UpdateTierHandlerTests
{
    private readonly IGenericRepository<ClientTier> _repository;
    private readonly ILogger<UpdateTierHandler> _logger;
    private readonly UpdateTierHandler _handler;

    public UpdateTierHandlerTests()
    {
        _repository = Substitute.For<IGenericRepository<ClientTier>>();
        _logger = Substitute.For<ILogger<UpdateTierHandler>>();
        _handler = new UpdateTierHandler(_repository, _logger);
    }

    [Fact]
    public async Task HandleAsync_UpdatesTier_WhenTierExists()
    {
        // Arrange
        var tierId = Guid.NewGuid();
        var existingTier = new ClientTier
        {
            Id = tierId,
            Name = "Old Name",
            SortOrder = 1,
            TenantId = Guid.NewGuid()
        };

        _repository.GetByIdAsync(tierId, Arg.Any<CancellationToken>()).Returns(existingTier);

        var command = new UpdateTier(
            TierId: tierId,
            Name: "New Name",
            SortOrder: 2
        );

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _repository.Received(1).UpdateAsync(Arg.Is<ClientTier>(t =>
            t.Name == "New Name" &&
            t.SortOrder == 2
        ), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ThrowsNotFoundException_WhenTierDoesNotExist()
    {
        // Arrange
        var tierId = Guid.NewGuid();
        _repository.GetByIdAsync(tierId, Arg.Any<CancellationToken>()).Returns((ClientTier?)null);

        var command = new UpdateTier(tierId, "New Name", 2);

        // Act
        var act = () => _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"*{tierId}*");
    }
}
