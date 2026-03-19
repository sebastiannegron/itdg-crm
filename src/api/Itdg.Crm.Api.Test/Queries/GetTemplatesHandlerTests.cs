namespace Itdg.Crm.Api.Test.Queries;

using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Application.QueryHandlers;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.Enums;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetTemplatesHandlerTests
{
    private readonly ITemplateRepository _repository;
    private readonly ILogger<GetTemplatesHandler> _logger;
    private readonly GetTemplatesHandler _handler;

    public GetTemplatesHandlerTests()
    {
        _repository = Substitute.For<ITemplateRepository>();
        _logger = Substitute.For<ILogger<GetTemplatesHandler>>();
        _handler = new GetTemplatesHandler(_repository, _logger);
    }

    [Fact]
    public async Task HandleAsync_ReturnsAllTemplates()
    {
        // Arrange
        var templates = new List<CommunicationTemplate>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = Guid.NewGuid(),
                Category = TemplateCategory.Onboarding,
                Name = "Template 1",
                SubjectTemplate = "Subject 1",
                BodyTemplate = "Body 1",
                Language = "en-pr",
                Version = 1,
                IsActive = true,
                CreatedById = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = Guid.NewGuid(),
                Category = TemplateCategory.General,
                Name = "Template 2",
                SubjectTemplate = "Subject 2",
                BodyTemplate = "Body 2",
                Language = "es-pr",
                Version = 2,
                IsActive = true,
                CreatedById = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            }
        };

        _repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(templates.AsReadOnly());

        // Act
        var result = await _handler.HandleAsync(new GetTemplates(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        var dtos = result.ToList();
        dtos.Should().HaveCount(2);
        dtos[0].Name.Should().Be("Template 1");
        dtos[1].Name.Should().Be("Template 2");
    }

    [Fact]
    public async Task HandleAsync_ReturnsEmpty_WhenNoTemplatesExist()
    {
        // Arrange
        _repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<CommunicationTemplate>().AsReadOnly());

        // Act
        var result = await _handler.HandleAsync(new GetTemplates(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }
}
