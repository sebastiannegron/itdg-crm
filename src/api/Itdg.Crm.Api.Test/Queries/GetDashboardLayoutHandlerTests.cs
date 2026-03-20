namespace Itdg.Crm.Api.Test.Queries;

using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Application.QueryHandlers;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetDashboardLayoutHandlerTests
{
    private readonly IDashboardLayoutRepository _repository;
    private readonly ILogger<GetDashboardLayoutHandler> _logger;
    private readonly GetDashboardLayoutHandler _handler;

    public GetDashboardLayoutHandlerTests()
    {
        _repository = Substitute.For<IDashboardLayoutRepository>();
        _logger = Substitute.For<ILogger<GetDashboardLayoutHandler>>();
        _handler = new GetDashboardLayoutHandler(_repository, _logger);
    }

    [Fact]
    public async Task HandleAsync_ReturnsLayout_WhenLayoutExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var layoutId = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow.AddDays(-1);
        var updatedAt = DateTimeOffset.UtcNow;
        var widgetConfig = "[{\"id\":\"stats\",\"order\":0},{\"id\":\"tasks\",\"order\":1}]";

        var layout = new DashboardLayout
        {
            Id = layoutId,
            UserId = userId,
            TenantId = Guid.NewGuid(),
            WidgetConfigurations = widgetConfig,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };

        _repository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(layout);

        // Act
        var result = await _handler.HandleAsync(new GetDashboardLayout(userId), Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(layoutId);
        result.UserId.Should().Be(userId);
        result.WidgetConfigurations.Should().Be(widgetConfig);
        result.CreatedAt.Should().Be(createdAt);
        result.UpdatedAt.Should().Be(updatedAt);
    }

    [Fact]
    public async Task HandleAsync_ReturnsNull_WhenNoLayoutExists()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _repository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((DashboardLayout?)null);

        // Act
        var result = await _handler.HandleAsync(new GetDashboardLayout(userId), Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task HandleAsync_MapsPropertiesCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var layoutId = Guid.NewGuid();

        var layout = new DashboardLayout
        {
            Id = layoutId,
            UserId = userId,
            TenantId = Guid.NewGuid(),
            WidgetConfigurations = null,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _repository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(layout);

        // Act
        var result = await _handler.HandleAsync(new GetDashboardLayout(userId), Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(layoutId);
        result.UserId.Should().Be(userId);
        result.WidgetConfigurations.Should().BeNull();
    }
}
