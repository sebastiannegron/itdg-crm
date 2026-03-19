namespace Itdg.Crm.Api.Test.Commands;

using Itdg.Crm.Api.Application.CommandHandlers;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.Enums;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class RetireTemplateHandlerTests
{
    private readonly ITemplateRepository _repository;
    private readonly ILogger<RetireTemplateHandler> _logger;
    private readonly RetireTemplateHandler _handler;

    public RetireTemplateHandlerTests()
    {
        _repository = Substitute.For<ITemplateRepository>();
        _logger = Substitute.For<ILogger<RetireTemplateHandler>>();
        _handler = new RetireTemplateHandler(_repository, _logger);
    }

    [Fact]
    public async Task HandleAsync_RetiresTemplate_WhenTemplateExists()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var existingTemplate = new CommunicationTemplate
        {
            Id = templateId,
            TenantId = Guid.NewGuid(),
            Category = TemplateCategory.General,
            Name = "Active Template",
            SubjectTemplate = "Subject",
            BodyTemplate = "Body",
            Language = "en-pr",
            Version = 1,
            IsActive = true,
            CreatedById = Guid.NewGuid()
        };

        _repository.GetByIdAsync(templateId, Arg.Any<CancellationToken>())
            .Returns(existingTemplate);

        var command = new RetireTemplate(templateId);

        // Act
        await _handler.HandleAsync(command, string.Empty, Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _repository.Received(1).UpdateAsync(
            Arg.Is<CommunicationTemplate>(t =>
                t.Id == templateId &&
                t.IsActive == false &&
                t.DeletedAt != null),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ThrowsNotFoundException_WhenTemplateDoesNotExist()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        _repository.GetByIdAsync(templateId, Arg.Any<CancellationToken>())
            .Returns((CommunicationTemplate?)null);

        var command = new RetireTemplate(templateId);

        // Act
        var act = () => _handler.HandleAsync(command, string.Empty, Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
