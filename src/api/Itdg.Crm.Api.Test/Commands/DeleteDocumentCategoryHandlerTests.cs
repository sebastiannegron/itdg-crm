namespace Itdg.Crm.Api.Test.Commands;

using Itdg.Crm.Api.Application.CommandHandlers;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class DeleteDocumentCategoryHandlerTests
{
    private readonly IGenericRepository<DocumentCategory> _repository;
    private readonly ILogger<DeleteDocumentCategoryHandler> _logger;
    private readonly DeleteDocumentCategoryHandler _handler;

    public DeleteDocumentCategoryHandlerTests()
    {
        _repository = Substitute.For<IGenericRepository<DocumentCategory>>();
        _logger = Substitute.For<ILogger<DeleteDocumentCategoryHandler>>();
        _handler = new DeleteDocumentCategoryHandler(_repository, _logger);
    }

    [Fact]
    public async Task HandleAsync_DeletesCategory_WhenCategoryExists()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var existing = new DocumentCategory
        {
            Id = categoryId,
            Name = "Tax Documents",
            NamingConvention = "{ClientName}_TaxDoc_{Date}",
            SortOrder = 1,
            IsDefault = false,
            TenantId = Guid.NewGuid()
        };

        _repository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>()).Returns(existing);

        var command = new DeleteDocumentCategory(categoryId);

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _repository.Received(1).DeleteAsync(existing, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ThrowsNotFoundException_WhenCategoryDoesNotExist()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _repository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>()).Returns((DocumentCategory?)null);

        var command = new DeleteDocumentCategory(categoryId);

        // Act
        var act = () => _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"*{categoryId}*");
    }
}
