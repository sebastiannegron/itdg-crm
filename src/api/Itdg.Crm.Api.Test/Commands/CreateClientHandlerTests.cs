namespace Itdg.Crm.Api.Test.Commands;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.CommandHandlers;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class CreateClientHandlerTests
{
    private readonly IClientRepository _repository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<CreateClientHandler> _logger;
    private readonly CreateClientHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    public CreateClientHandlerTests()
    {
        _repository = Substitute.For<IClientRepository>();
        _tenantProvider = Substitute.For<ITenantProvider>();
        _logger = Substitute.For<ILogger<CreateClientHandler>>();
        _tenantProvider.GetTenantId().Returns(_tenantId);
        _handler = new CreateClientHandler(_repository, _tenantProvider, _logger);
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
}
