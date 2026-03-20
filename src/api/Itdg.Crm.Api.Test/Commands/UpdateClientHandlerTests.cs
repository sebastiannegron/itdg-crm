namespace Itdg.Crm.Api.Test.Commands;

using Itdg.Crm.Api.Application.CommandHandlers;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class UpdateClientHandlerTests
{
    private readonly IClientRepository _repository;
    private readonly ILogger<UpdateClientHandler> _logger;
    private readonly UpdateClientHandler _handler;

    public UpdateClientHandlerTests()
    {
        _repository = Substitute.For<IClientRepository>();
        _logger = Substitute.For<ILogger<UpdateClientHandler>>();
        _handler = new UpdateClientHandler(_repository, _logger);
    }

    [Fact]
    public async Task HandleAsync_UpdatesClient_WhenClientExists()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        var existingClient = new Client
        {
            Id = clientId,
            Name = "Old Name",
            ContactEmail = "old@example.com",
            Status = ClientStatus.Active,
            TenantId = Guid.NewGuid()
        };

        _repository.GetByIdAsync(clientId, Arg.Any<CancellationToken>()).Returns(existingClient);

        var command = new UpdateClient(
            ClientId: clientId,
            Name: "New Name",
            ContactEmail: "new@example.com",
            Phone: "787-555-9999",
            Address: "456 New St",
            TierId: Guid.NewGuid(),
            Status: ClientStatus.Inactive,
            IndustryTag: "Finance",
            Notes: "Updated notes",
            CustomFields: null
        );

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _repository.Received(1).UpdateAsync(Arg.Is<Client>(c =>
            c.Name == "New Name" &&
            c.ContactEmail == "new@example.com" &&
            c.Phone == "787-555-9999" &&
            c.Address == "456 New St" &&
            c.Status == ClientStatus.Inactive &&
            c.IndustryTag == "Finance" &&
            c.Notes == "Updated notes" &&
            c.CustomFields == null
        ), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ThrowsNotFoundException_WhenClientDoesNotExist()
    {
        // Arrange
        var clientId = Guid.NewGuid();
        _repository.GetByIdAsync(clientId, Arg.Any<CancellationToken>()).Returns((Client?)null);

        var command = new UpdateClient(
            ClientId: clientId,
            Name: "New Name",
            ContactEmail: null,
            Phone: null,
            Address: null,
            TierId: null,
            Status: ClientStatus.Active,
            IndustryTag: null,
            Notes: null,
            CustomFields: null
        );

        // Act
        var act = () => _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"*{clientId}*");
    }
}
