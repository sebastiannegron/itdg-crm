namespace Itdg.Crm.Api.Test.Queries;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Application.QueryHandlers;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.Enums;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class RenderTemplateHandlerTests
{
    private readonly ITemplateRepository _repository;
    private readonly ITemplateRenderer _renderer;
    private readonly ILogger<RenderTemplateHandler> _logger;
    private readonly RenderTemplateHandler _handler;

    public RenderTemplateHandlerTests()
    {
        _repository = Substitute.For<ITemplateRepository>();
        _renderer = Substitute.For<ITemplateRenderer>();
        _logger = Substitute.For<ILogger<RenderTemplateHandler>>();
        _handler = new RenderTemplateHandler(_repository, _renderer, _logger);
    }

    [Fact]
    public async Task HandleAsync_RendersTemplate_WhenTemplateExists()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var template = new CommunicationTemplate
        {
            Id = templateId,
            TenantId = Guid.NewGuid(),
            Category = TemplateCategory.PaymentReminder,
            Name = "Payment Reminder EN",
            SubjectTemplate = "Payment Due {{due_date}}",
            BodyTemplate = "Dear {{client_name}}, your payment of {{amount}} is due on {{due_date}}.",
            Language = "en-pr",
            Version = 1,
            IsActive = true,
            CreatedById = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var mergeFields = new Dictionary<string, string>
        {
            { "client_name", "John Doe" },
            { "due_date", "2026-04-01" },
            { "amount", "$500.00" }
        };

        _repository.GetByIdAsync(templateId, Arg.Any<CancellationToken>())
            .Returns(template);

        _renderer.Render(template.SubjectTemplate, Arg.Any<IDictionary<string, string>>())
            .Returns("Payment Due 2026-04-01");

        _renderer.Render(template.BodyTemplate, Arg.Any<IDictionary<string, string>>())
            .Returns("Dear John Doe, your payment of $500.00 is due on 2026-04-01.");

        // Act
        var result = await _handler.HandleAsync(
            new RenderTemplate(templateId, mergeFields),
            Guid.NewGuid(),
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Subject.Should().Be("Payment Due 2026-04-01");
        result.Body.Should().Be("Dear John Doe, your payment of $500.00 is due on 2026-04-01.");
    }

    [Fact]
    public async Task HandleAsync_ThrowsNotFoundException_WhenTemplateDoesNotExist()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        _repository.GetByIdAsync(templateId, Arg.Any<CancellationToken>())
            .Returns((CommunicationTemplate?)null);

        var mergeFields = new Dictionary<string, string> { { "client_name", "Test" } };

        // Act
        var act = () => _handler.HandleAsync(
            new RenderTemplate(templateId, mergeFields),
            Guid.NewGuid(),
            CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_CallsRendererWithCorrectTemplates()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var template = new CommunicationTemplate
        {
            Id = templateId,
            TenantId = Guid.NewGuid(),
            Category = TemplateCategory.General,
            Name = "Welcome ES",
            SubjectTemplate = "Bienvenido {{client_name}}",
            BodyTemplate = "Hola {{client_name}}, bienvenido a nuestro servicio.",
            Language = "es-pr",
            Version = 1,
            IsActive = true,
            CreatedById = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var mergeFields = new Dictionary<string, string> { { "client_name", "María García" } };

        _repository.GetByIdAsync(templateId, Arg.Any<CancellationToken>())
            .Returns(template);

        _renderer.Render(Arg.Any<string>(), Arg.Any<IDictionary<string, string>>())
            .Returns(x => x.Arg<string>());

        // Act
        await _handler.HandleAsync(
            new RenderTemplate(templateId, mergeFields),
            Guid.NewGuid(),
            CancellationToken.None);

        // Assert
        _renderer.Received(1).Render(template.SubjectTemplate, Arg.Any<IDictionary<string, string>>());
        _renderer.Received(1).Render(template.BodyTemplate, Arg.Any<IDictionary<string, string>>());
    }
}
