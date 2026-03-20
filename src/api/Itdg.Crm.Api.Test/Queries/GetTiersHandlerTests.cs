namespace Itdg.Crm.Api.Test.Queries;

using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Application.QueryHandlers;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetTiersHandlerTests
{
    private readonly IGenericRepository<ClientTier> _repository;
    private readonly ILogger<GetTiersHandler> _logger;
    private readonly GetTiersHandler _handler;

    public GetTiersHandlerTests()
    {
        _repository = Substitute.For<IGenericRepository<ClientTier>>();
        _logger = Substitute.For<ILogger<GetTiersHandler>>();
        _handler = new GetTiersHandler(_repository, _logger);
    }

    [Fact]
    public async Task HandleAsync_ReturnsAllTiers_OrderedBySortOrder()
    {
        // Arrange
        var tiers = new List<ClientTier>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = Guid.NewGuid(),
                Name = "Tier 3",
                SortOrder = 3,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = Guid.NewGuid(),
                Name = "Tier 1",
                SortOrder = 1,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = Guid.NewGuid(),
                Name = "Tier 2",
                SortOrder = 2,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            }
        };

        _repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(tiers.AsReadOnly());

        // Act
        var result = await _handler.HandleAsync(new GetTiers(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        var dtos = result.ToList();
        dtos.Should().HaveCount(3);
        dtos[0].Name.Should().Be("Tier 1");
        dtos[1].Name.Should().Be("Tier 2");
        dtos[2].Name.Should().Be("Tier 3");
    }

    [Fact]
    public async Task HandleAsync_ReturnsEmpty_WhenNoTiersExist()
    {
        // Arrange
        _repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<ClientTier>().AsReadOnly());

        // Act
        var result = await _handler.HandleAsync(new GetTiers(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_MapsPropertiesCorrectly()
    {
        // Arrange
        var tierId = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow.AddDays(-1);
        var updatedAt = DateTimeOffset.UtcNow;

        var tiers = new List<ClientTier>
        {
            new()
            {
                Id = tierId,
                TenantId = Guid.NewGuid(),
                Name = "Premium",
                SortOrder = 1,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt
            }
        };

        _repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(tiers.AsReadOnly());

        // Act
        var result = await _handler.HandleAsync(new GetTiers(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        var dto = result.Single();
        dto.TierId.Should().Be(tierId);
        dto.Name.Should().Be("Premium");
        dto.SortOrder.Should().Be(1);
        dto.CreatedAt.Should().Be(createdAt);
        dto.UpdatedAt.Should().Be(updatedAt);
    }
}
