namespace Itdg.Crm.Api.Test.Commands;

using Itdg.Crm.Api.Application.CommandHandlers;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.Enums;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class UpdateTemplateHandlerTests
{
    private readonly ITemplateRepository _repository;
    private readonly ILogger<UpdateTemplateHandler> _logger;
    private readonly UpdateTemplateHandler _handler;

    public UpdateTemplateHandlerTests()
    {
        _repository = Substitute.For<ITemplateRepository>();
        _logger = Substitute.For<ILogger<UpdateTemplateHandler>>();
        _handler = new UpdateTemplateHandler(_repository, _logger);
    }

    [Fact]
    public async Task HandleAsync_UpdatesTemplate_WhenTemplateExists()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var existingTemplate = new CommunicationTemplate
        {
            Id = templateId,
            TenantId = Guid.NewGuid(),
            Category = TemplateCategory.General,
            Name = "Original Name",
            SubjectTemplate = "Original Subject",
            BodyTemplate = "Original Body",
            Language = "en-pr",
            Version = 1,
            IsActive = true,
            CreatedById = Guid.NewGuid()
        };

        _repository.GetByIdAsync(templateId, Arg.Any<CancellationToken>())
            .Returns(existingTemplate);

        var command = new UpdateTemplate(
            templateId,
            TemplateCategory.Onboarding,
            "Updated Name",
            "Updated Subject",
            "Updated Body",
            "es-pr"
        );

        // Act
        await _handler.HandleAsync(command, "es-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _repository.Received(1).UpdateAsync(
            Arg.Is<CommunicationTemplate>(t =>
                t.Id == templateId &&
                t.Name == "Updated Name" &&
                t.Category == TemplateCategory.Onboarding &&
                t.SubjectTemplate == "Updated Subject" &&
                t.BodyTemplate == "Updated Body" &&
                t.Language == "es-pr" &&
                t.Version == 2),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ThrowsNotFoundException_WhenTemplateDoesNotExist()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        _repository.GetByIdAsync(templateId, Arg.Any<CancellationToken>())
            .Returns((CommunicationTemplate?)null);

        var command = new UpdateTemplate(
            templateId,
            TemplateCategory.General,
            "Updated Name",
            "Updated Subject",
            "Updated Body",
            "en-pr"
        );

        // Act
        var act = () => _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
