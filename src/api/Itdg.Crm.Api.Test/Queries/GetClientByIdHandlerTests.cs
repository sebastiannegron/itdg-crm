namespace Itdg.Crm.Api.Test.Queries;

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
    private readonly ILogger<GetClientByIdHandler> _logger;
    private readonly GetClientByIdHandler _handler;

    public GetClientByIdHandlerTests()
    {
        _repository = Substitute.For<IClientRepository>();
        _logger = Substitute.For<ILogger<GetClientByIdHandler>>();
        _handler = new GetClientByIdHandler(_repository, _logger);
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
}
