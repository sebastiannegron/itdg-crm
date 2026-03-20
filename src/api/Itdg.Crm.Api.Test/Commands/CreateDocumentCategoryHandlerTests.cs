namespace Itdg.Crm.Api.Test.Commands;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.CommandHandlers;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class CreateDocumentCategoryHandlerTests
{
    private readonly IGenericRepository<DocumentCategory> _repository;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<CreateDocumentCategoryHandler> _logger;
    private readonly CreateDocumentCategoryHandler _handler;
    private readonly Guid _tenantId = Guid.NewGuid();

    public CreateDocumentCategoryHandlerTests()
    {
        _repository = Substitute.For<IGenericRepository<DocumentCategory>>();
        _tenantProvider = Substitute.For<ITenantProvider>();
        _logger = Substitute.For<ILogger<CreateDocumentCategoryHandler>>();
        _tenantProvider.GetTenantId().Returns(_tenantId);
        _handler = new CreateDocumentCategoryHandler(_repository, _tenantProvider, _logger);
    }

    [Fact]
    public async Task HandleAsync_CreatesCategory_WithCorrectProperties()
    {
        // Arrange
        var command = new CreateDocumentCategory(
            Name: "Tax Documents",
            NamingConvention: "{ClientName}_TaxDoc_{Date}",
            SortOrder: 1
        );

        DocumentCategory? captured = null;
        await _repository.AddAsync(Arg.Do<DocumentCategory>(c => captured = c), Arg.Any<CancellationToken>());

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _repository.Received(1).AddAsync(Arg.Any<DocumentCategory>(), Arg.Any<CancellationToken>());
        captured.Should().NotBeNull();
        captured!.Name.Should().Be("Tax Documents");
        captured.NamingConvention.Should().Be("{ClientName}_TaxDoc_{Date}");
        captured.SortOrder.Should().Be(1);
        captured.IsDefault.Should().BeFalse();
        captured.TenantId.Should().Be(_tenantId);
        captured.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task HandleAsync_SetsTenantId_FromTenantProvider()
    {
        // Arrange
        var command = new CreateDocumentCategory(
            Name: "Invoices",
            NamingConvention: null,
            SortOrder: 2
        );

        DocumentCategory? captured = null;
        await _repository.AddAsync(Arg.Do<DocumentCategory>(c => captured = c), Arg.Any<CancellationToken>());

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        captured.Should().NotBeNull();
        captured!.TenantId.Should().Be(_tenantId);
    }

    [Fact]
    public async Task HandleAsync_AllowsNullNamingConvention()
    {
        // Arrange
        var command = new CreateDocumentCategory(
            Name: "General",
            NamingConvention: null,
            SortOrder: 3
        );

        DocumentCategory? captured = null;
        await _repository.AddAsync(Arg.Do<DocumentCategory>(c => captured = c), Arg.Any<CancellationToken>());

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        captured.Should().NotBeNull();
        captured!.NamingConvention.Should().BeNull();
    }
}
