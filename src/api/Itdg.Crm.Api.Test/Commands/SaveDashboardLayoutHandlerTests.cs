namespace Itdg.Crm.Api.Test.Commands;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.CommandHandlers;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class SaveDashboardLayoutHandlerTests
{
    private readonly IDashboardLayoutRepository _repository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<SaveDashboardLayoutHandler> _logger;
    private readonly SaveDashboardLayoutHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    public SaveDashboardLayoutHandlerTests()
    {
        _repository = Substitute.For<IDashboardLayoutRepository>();
        _tenantProvider = Substitute.For<ITenantProvider>();
        _logger = Substitute.For<ILogger<SaveDashboardLayoutHandler>>();
        _tenantProvider.GetTenantId().Returns(_tenantId);
        _handler = new SaveDashboardLayoutHandler(_repository, _tenantProvider, _logger);
    }

    [Fact]
    public async Task HandleAsync_CreatesNewLayout_WhenNoExistingLayout()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var widgetConfig = "[{\"id\":\"stats\",\"order\":0},{\"id\":\"tasks\",\"order\":1}]";
        var command = new SaveDashboardLayout(userId, widgetConfig);

        _repository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((DashboardLayout?)null);

        DashboardLayout? capturedLayout = null;
        await _repository.AddAsync(Arg.Do<DashboardLayout>(l => capturedLayout = l), Arg.Any<CancellationToken>());

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _repository.Received(1).AddAsync(Arg.Any<DashboardLayout>(), Arg.Any<CancellationToken>());
        await _repository.DidNotReceive().UpdateAsync(Arg.Any<DashboardLayout>(), Arg.Any<CancellationToken>());
        capturedLayout.Should().NotBeNull();
        capturedLayout!.UserId.Should().Be(userId);
        capturedLayout.WidgetConfigurations.Should().Be(widgetConfig);
        capturedLayout.TenantId.Should().Be(_tenantId);
        capturedLayout.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task HandleAsync_UpdatesExistingLayout_WhenLayoutExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var existingLayout = new DashboardLayout
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TenantId = _tenantId,
            WidgetConfigurations = "[{\"id\":\"stats\",\"order\":0}]",
            CreatedAt = DateTimeOffset.UtcNow.AddDays(-1),
            UpdatedAt = DateTimeOffset.UtcNow.AddDays(-1)
        };

        var newConfig = "[{\"id\":\"tasks\",\"order\":0},{\"id\":\"stats\",\"order\":1}]";
        var command = new SaveDashboardLayout(userId, newConfig);

        _repository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns(existingLayout);

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _repository.Received(1).UpdateAsync(existingLayout, Arg.Any<CancellationToken>());
        await _repository.DidNotReceive().AddAsync(Arg.Any<DashboardLayout>(), Arg.Any<CancellationToken>());
        existingLayout.WidgetConfigurations.Should().Be(newConfig);
    }

    [Fact]
    public async Task HandleAsync_SetsTenantId_FromTenantProvider()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new SaveDashboardLayout(userId, "[]");

        _repository.GetByUserIdAsync(userId, Arg.Any<CancellationToken>())
            .Returns((DashboardLayout?)null);

        DashboardLayout? capturedLayout = null;
        await _repository.AddAsync(Arg.Do<DashboardLayout>(l => capturedLayout = l), Arg.Any<CancellationToken>());

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        capturedLayout.Should().NotBeNull();
        capturedLayout!.TenantId.Should().Be(_tenantId);
    }
}
