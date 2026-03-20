namespace Itdg.Crm.Api.Test.Commands;

using Itdg.Crm.Api.Application.CommandHandlers;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class UpdateDocumentCategoryHandlerTests
{
    private readonly IGenericRepository<DocumentCategory> _repository;
    private readonly ILogger<UpdateDocumentCategoryHandler> _logger;
    private readonly UpdateDocumentCategoryHandler _handler;

    public UpdateDocumentCategoryHandlerTests()
    {
        _repository = Substitute.For<IGenericRepository<DocumentCategory>>();
        _logger = Substitute.For<ILogger<UpdateDocumentCategoryHandler>>();
        _handler = new UpdateDocumentCategoryHandler(_repository, _logger);
    }

    [Fact]
    public async Task HandleAsync_UpdatesCategory_WhenCategoryExists()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var existing = new DocumentCategory
        {
            Id = categoryId,
            Name = "Old Name",
            NamingConvention = "{ClientName}_Old_{Date}",
            SortOrder = 1,
            IsDefault = false,
            TenantId = Guid.NewGuid()
        };

        _repository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>()).Returns(existing);

        var command = new UpdateDocumentCategory(
            CategoryId: categoryId,
            Name: "New Name",
            NamingConvention: "{ClientName}_New_{Date}",
            SortOrder: 2
        );

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _repository.Received(1).UpdateAsync(Arg.Is<DocumentCategory>(c =>
            c.Name == "New Name" &&
            c.NamingConvention == "{ClientName}_New_{Date}" &&
            c.SortOrder == 2
        ), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ThrowsNotFoundException_WhenCategoryDoesNotExist()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        _repository.GetByIdAsync(categoryId, Arg.Any<CancellationToken>()).Returns((DocumentCategory?)null);

        var command = new UpdateDocumentCategory(categoryId, "New Name", null, 1);

        // Act
        var act = () => _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"*{categoryId}*");
    }
}
