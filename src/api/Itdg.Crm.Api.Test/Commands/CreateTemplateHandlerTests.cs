namespace Itdg.Crm.Api.Test.Commands;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.CommandHandlers;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.Enums;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class CreateTemplateHandlerTests
{
    private readonly ITemplateRepository _repository;
    private readonly ILogger<CreateTemplateHandler> _logger;
    private readonly CreateTemplateHandler _handler;

    public CreateTemplateHandlerTests()
    {
        _repository = Substitute.For<ITemplateRepository>();
        _logger = Substitute.For<ILogger<CreateTemplateHandler>>();
        _handler = new CreateTemplateHandler(_repository, _logger);
    }

    [Fact]
    public async Task HandleAsync_CreatesTemplate_WhenCommandIsValid()
    {
        // Arrange
        var command = new CreateTemplate(
            TemplateCategory.Onboarding,
            "Welcome Template",
            "Welcome {{client_name}}",
            "Hello {{client_name}}, welcome!",
            "en-pr",
            Guid.NewGuid()
        );

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _repository.Received(1).AddAsync(
            Arg.Is<CommunicationTemplate>(t =>
                t.Name == "Welcome Template" &&
                t.Category == TemplateCategory.Onboarding &&
                t.SubjectTemplate == "Welcome {{client_name}}" &&
                t.BodyTemplate == "Hello {{client_name}}, welcome!" &&
                t.Language == "en-pr" &&
                t.Version == 1 &&
                t.IsActive),
            Arg.Any<CancellationToken>());
    }
}
