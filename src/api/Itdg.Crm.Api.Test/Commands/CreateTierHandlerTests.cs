namespace Itdg.Crm.Api.Test.Commands;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.CommandHandlers;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class CreateTierHandlerTests
{
    private readonly IGenericRepository<ClientTier> _repository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<CreateTierHandler> _logger;
    private readonly CreateTierHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    public CreateTierHandlerTests()
    {
        _repository = Substitute.For<IGenericRepository<ClientTier>>();
        _tenantProvider = Substitute.For<ITenantProvider>();
        _logger = Substitute.For<ILogger<CreateTierHandler>>();
        _tenantProvider.GetTenantId().Returns(_tenantId);
        _handler = new CreateTierHandler(_repository, _tenantProvider, _logger);
    }

    [Fact]
    public async Task HandleAsync_CreatesTier_WithCorrectProperties()
    {
        // Arrange
        var command = new CreateTier(
            Name: "Tier 1",
            SortOrder: 1
        );

        ClientTier? capturedTier = null;
        await _repository.AddAsync(Arg.Do<ClientTier>(t => capturedTier = t), Arg.Any<CancellationToken>());

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _repository.Received(1).AddAsync(Arg.Any<ClientTier>(), Arg.Any<CancellationToken>());
        capturedTier.Should().NotBeNull();
        capturedTier!.Name.Should().Be("Tier 1");
        capturedTier.SortOrder.Should().Be(1);
        capturedTier.TenantId.Should().Be(_tenantId);
        capturedTier.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task HandleAsync_SetsTenantId_FromTenantProvider()
    {
        // Arrange
        var command = new CreateTier(
            Name: "Tier 2",
            SortOrder: 2
        );

        ClientTier? capturedTier = null;
        await _repository.AddAsync(Arg.Do<ClientTier>(t => capturedTier = t), Arg.Any<CancellationToken>());

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        capturedTier.Should().NotBeNull();
        capturedTier!.TenantId.Should().Be(_tenantId);
    }
}
