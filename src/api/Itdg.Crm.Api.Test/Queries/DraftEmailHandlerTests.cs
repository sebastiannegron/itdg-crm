namespace Itdg.Crm.Api.Test.Queries;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Application.QueryHandlers;
using Microsoft.Extensions.Logging;

public class DraftEmailHandlerTests
{
    private readonly IAiDraftingService _aiDraftingService;
    private readonly ILogger<DraftEmailHandler> _logger;
    private readonly DraftEmailHandler _handler;

    public DraftEmailHandlerTests()
    {
        _aiDraftingService = Substitute.For<IAiDraftingService>();
        _logger = Substitute.For<ILogger<DraftEmailHandler>>();
        _handler = new DraftEmailHandler(_aiDraftingService, _logger);
    }

    [Fact]
    public async Task HandleAsync_ReturnsDraft_WhenServiceSucceeds()
    {
        // Arrange
        var query = new DraftEmail("John Doe", "Tax return filing", "en");
        var expectedDraft = "Dear John Doe,\n\nRegarding your tax return filing...";

        _aiDraftingService.GenerateDraftAsync(
            Arg.Is<AiDraftRequest>(r =>
                r.ClientName == "John Doe" &&
                r.Topic == "Tax return filing" &&
                r.Language == "en"),
            Arg.Any<CancellationToken>())
            .Returns(expectedDraft);

        // Act
        var result = await _handler.HandleAsync(query, Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().Be(expectedDraft);
    }

    [Fact]
    public async Task HandleAsync_PassesAdditionalContext_WhenProvided()
    {
        // Arrange
        var query = new DraftEmail("María García", "Fecha límite", "es", "Outstanding balance of $5,000");
        var expectedDraft = "Estimada María García,\n\nCon respecto a la fecha límite...";

        _aiDraftingService.GenerateDraftAsync(
            Arg.Is<AiDraftRequest>(r =>
                r.ClientName == "María García" &&
                r.Topic == "Fecha límite" &&
                r.Language == "es" &&
                r.AdditionalContext == "Outstanding balance of $5,000"),
            Arg.Any<CancellationToken>())
            .Returns(expectedDraft);

        // Act
        var result = await _handler.HandleAsync(query, Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().Be(expectedDraft);
        await _aiDraftingService.Received(1).GenerateDraftAsync(
            Arg.Is<AiDraftRequest>(r => r.AdditionalContext == "Outstanding balance of $5,000"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_CallsService_WithCorrectAiDraftRequest()
    {
        // Arrange
        var query = new DraftEmail("Test Client", "Payment reminder", "en-pr");

        _aiDraftingService.GenerateDraftAsync(Arg.Any<AiDraftRequest>(), Arg.Any<CancellationToken>())
            .Returns("Draft content");

        // Act
        await _handler.HandleAsync(query, Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _aiDraftingService.Received(1).GenerateDraftAsync(
            Arg.Is<AiDraftRequest>(r =>
                r.ClientName == "Test Client" &&
                r.Topic == "Payment reminder" &&
                r.Language == "en-pr" &&
                r.AdditionalContext == null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_PropagatesException_WhenServiceFails()
    {
        // Arrange
        var query = new DraftEmail("John Doe", "Tax filing", "en");

        _aiDraftingService.GenerateDraftAsync(Arg.Any<AiDraftRequest>(), Arg.Any<CancellationToken>())
            .Returns<string>(x => throw new InvalidOperationException("AI service unavailable"));

        // Act
        Func<Task> act = async () => await _handler.HandleAsync(query, Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("AI service unavailable");
    }
}
