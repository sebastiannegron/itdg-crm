namespace Itdg.Crm.Api.Test.Queries;

using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Application.QueryHandlers;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.Enums;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetTemplateByIdHandlerTests
{
    private readonly ITemplateRepository _repository;
    private readonly ILogger<GetTemplateByIdHandler> _logger;
    private readonly GetTemplateByIdHandler _handler;

    public GetTemplateByIdHandlerTests()
    {
        _repository = Substitute.For<ITemplateRepository>();
        _logger = Substitute.For<ILogger<GetTemplateByIdHandler>>();
        _handler = new GetTemplateByIdHandler(_repository, _logger);
    }

    [Fact]
    public async Task HandleAsync_ReturnsTemplate_WhenTemplateExists()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var createdById = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow;

        var template = new CommunicationTemplate
        {
            Id = templateId,
            TenantId = Guid.NewGuid(),
            Category = TemplateCategory.PaymentReminder,
            Name = "Payment Reminder",
            SubjectTemplate = "Payment Due {{due_date}}",
            BodyTemplate = "Dear {{client_name}}, your payment is due.",
            Language = "en-pr",
            Version = 3,
            IsActive = true,
            CreatedById = createdById,
            CreatedAt = createdAt,
            UpdatedAt = createdAt
        };

        _repository.GetByIdAsync(templateId, Arg.Any<CancellationToken>())
            .Returns(template);

        // Act
        var result = await _handler.HandleAsync(new GetTemplateById(templateId), Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(templateId);
        result.Name.Should().Be("Payment Reminder");
        result.Category.Should().Be(TemplateCategory.PaymentReminder);
        result.Version.Should().Be(3);
        result.CreatedById.Should().Be(createdById);
    }

    [Fact]
    public async Task HandleAsync_ThrowsNotFoundException_WhenTemplateDoesNotExist()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        _repository.GetByIdAsync(templateId, Arg.Any<CancellationToken>())
            .Returns((CommunicationTemplate?)null);

        // Act
        var act = () => _handler.HandleAsync(new GetTemplateById(templateId), Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
