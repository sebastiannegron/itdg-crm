namespace Itdg.Crm.Api.Test.Queries;

using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Application.QueryHandlers;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetDashboardSummaryHandlerTests
{
    private readonly IDashboardRepository _repository;
    private readonly ILogger<GetDashboardSummaryHandler> _logger;
    private readonly GetDashboardSummaryHandler _handler;

    public GetDashboardSummaryHandlerTests()
    {
        _repository = Substitute.For<IDashboardRepository>();
        _logger = Substitute.For<ILogger<GetDashboardSummaryHandler>>();
        _handler = new GetDashboardSummaryHandler(_repository, _logger);
    }

    [Fact]
    public async Task HandleAsync_ReturnsDashboardSummary_WithAllMetrics()
    {
        // Arrange
        _repository.GetTotalClientsCountAsync(Arg.Any<CancellationToken>()).Returns(10);
        _repository.GetClientCountsByStatusAsync(Arg.Any<CancellationToken>())
            .Returns(new List<(ClientStatus Status, int Count)>
            {
                (ClientStatus.Active, 7),
                (ClientStatus.Inactive, 2),
                (ClientStatus.Suspended, 1)
            });
        _repository.GetClientCountsByTierAsync(Arg.Any<CancellationToken>())
            .Returns(new List<(Guid? TierId, string? TierName, int Count)>
            {
                (Guid.NewGuid(), "Premium", 5),
                (Guid.NewGuid(), "Standard", 3),
                (null, null, 2)
            });
        _repository.GetPendingTasksCountAsync(Arg.Any<CancellationToken>()).Returns(3);
        _repository.GetRecentEscalationsCountAsync(Arg.Any<CancellationToken>()).Returns(2);
        _repository.GetUpcomingDeadlinesCountAsync(Arg.Any<CancellationToken>()).Returns(5);
        _repository.GetUnreadNotificationsCountAsync(Arg.Any<CancellationToken>()).Returns(8);

        // Act
        var result = await _handler.HandleAsync(new GetDashboardSummary(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalClients.Should().Be(10);
        result.ClientsByStatus.Should().HaveCount(3);
        result.ClientsByStatus.Should().Contain(c => c.Status == "Active" && c.Count == 7);
        result.ClientsByStatus.Should().Contain(c => c.Status == "Inactive" && c.Count == 2);
        result.ClientsByStatus.Should().Contain(c => c.Status == "Suspended" && c.Count == 1);
        result.ClientsByTier.Should().HaveCount(3);
        result.ClientsByTier.Should().Contain(c => c.TierName == "Premium" && c.Count == 5);
        result.ClientsByTier.Should().Contain(c => c.TierName == "Standard" && c.Count == 3);
        result.ClientsByTier.Should().Contain(c => c.TierName == null && c.Count == 2);
        result.PendingTasksCount.Should().Be(3);
        result.RecentEscalationsCount.Should().Be(2);
        result.UpcomingDeadlinesCount.Should().Be(5);
        result.UnreadNotificationsCount.Should().Be(8);
    }

    [Fact]
    public async Task HandleAsync_ReturnsZeroCounts_WhenNoDataExists()
    {
        // Arrange
        _repository.GetTotalClientsCountAsync(Arg.Any<CancellationToken>()).Returns(0);
        _repository.GetClientCountsByStatusAsync(Arg.Any<CancellationToken>())
            .Returns(new List<(ClientStatus Status, int Count)>());
        _repository.GetClientCountsByTierAsync(Arg.Any<CancellationToken>())
            .Returns(new List<(Guid? TierId, string? TierName, int Count)>());
        _repository.GetPendingTasksCountAsync(Arg.Any<CancellationToken>()).Returns(0);
        _repository.GetRecentEscalationsCountAsync(Arg.Any<CancellationToken>()).Returns(0);
        _repository.GetUpcomingDeadlinesCountAsync(Arg.Any<CancellationToken>()).Returns(0);
        _repository.GetUnreadNotificationsCountAsync(Arg.Any<CancellationToken>()).Returns(0);

        // Act
        var result = await _handler.HandleAsync(new GetDashboardSummary(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TotalClients.Should().Be(0);
        result.ClientsByStatus.Should().BeEmpty();
        result.ClientsByTier.Should().BeEmpty();
        result.PendingTasksCount.Should().Be(0);
        result.RecentEscalationsCount.Should().Be(0);
        result.UpcomingDeadlinesCount.Should().Be(0);
        result.UnreadNotificationsCount.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_CallsAllRepositoryMethods()
    {
        // Arrange
        _repository.GetTotalClientsCountAsync(Arg.Any<CancellationToken>()).Returns(0);
        _repository.GetClientCountsByStatusAsync(Arg.Any<CancellationToken>())
            .Returns(new List<(ClientStatus Status, int Count)>());
        _repository.GetClientCountsByTierAsync(Arg.Any<CancellationToken>())
            .Returns(new List<(Guid? TierId, string? TierName, int Count)>());
        _repository.GetPendingTasksCountAsync(Arg.Any<CancellationToken>()).Returns(0);
        _repository.GetRecentEscalationsCountAsync(Arg.Any<CancellationToken>()).Returns(0);
        _repository.GetUpcomingDeadlinesCountAsync(Arg.Any<CancellationToken>()).Returns(0);
        _repository.GetUnreadNotificationsCountAsync(Arg.Any<CancellationToken>()).Returns(0);

        // Act
        await _handler.HandleAsync(new GetDashboardSummary(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _repository.Received(1).GetTotalClientsCountAsync(Arg.Any<CancellationToken>());
        await _repository.Received(1).GetClientCountsByStatusAsync(Arg.Any<CancellationToken>());
        await _repository.Received(1).GetClientCountsByTierAsync(Arg.Any<CancellationToken>());
        await _repository.Received(1).GetPendingTasksCountAsync(Arg.Any<CancellationToken>());
        await _repository.Received(1).GetRecentEscalationsCountAsync(Arg.Any<CancellationToken>());
        await _repository.Received(1).GetUpcomingDeadlinesCountAsync(Arg.Any<CancellationToken>());
        await _repository.Received(1).GetUnreadNotificationsCountAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_MapsClientStatusToString()
    {
        // Arrange
        _repository.GetTotalClientsCountAsync(Arg.Any<CancellationToken>()).Returns(1);
        _repository.GetClientCountsByStatusAsync(Arg.Any<CancellationToken>())
            .Returns(new List<(ClientStatus Status, int Count)>
            {
                (ClientStatus.Active, 1)
            });
        _repository.GetClientCountsByTierAsync(Arg.Any<CancellationToken>())
            .Returns(new List<(Guid? TierId, string? TierName, int Count)>());
        _repository.GetPendingTasksCountAsync(Arg.Any<CancellationToken>()).Returns(0);
        _repository.GetRecentEscalationsCountAsync(Arg.Any<CancellationToken>()).Returns(0);
        _repository.GetUpcomingDeadlinesCountAsync(Arg.Any<CancellationToken>()).Returns(0);
        _repository.GetUnreadNotificationsCountAsync(Arg.Any<CancellationToken>()).Returns(0);

        // Act
        var result = await _handler.HandleAsync(new GetDashboardSummary(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.ClientsByStatus.Should().HaveCount(1);
        result.ClientsByStatus[0].Status.Should().Be("Active");
        result.ClientsByStatus[0].Count.Should().Be(1);
    }
}
