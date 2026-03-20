namespace Itdg.Crm.Api.Test.Commands;

using Itdg.Crm.Api.Application.CommandHandlers;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class ReorderDocumentCategoriesHandlerTests
{
    private readonly IGenericRepository<DocumentCategory> _repository;
    private readonly ILogger<ReorderDocumentCategoriesHandler> _logger;
    private readonly ReorderDocumentCategoriesHandler _handler;

    public ReorderDocumentCategoriesHandlerTests()
    {
        _repository = Substitute.For<IGenericRepository<DocumentCategory>>();
        _logger = Substitute.For<ILogger<ReorderDocumentCategoriesHandler>>();
        _handler = new ReorderDocumentCategoriesHandler(_repository, _logger);
    }

    [Fact]
    public async Task HandleAsync_ReordersCategories_WhenAllExist()
    {
        // Arrange
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();

        var cat1 = new DocumentCategory
        {
            Id = id1, Name = "Cat A", SortOrder = 1, IsDefault = false, TenantId = Guid.NewGuid()
        };
        var cat2 = new DocumentCategory
        {
            Id = id2, Name = "Cat B", SortOrder = 2, IsDefault = false, TenantId = Guid.NewGuid()
        };

        _repository.GetByIdAsync(id1, Arg.Any<CancellationToken>()).Returns(cat1);
        _repository.GetByIdAsync(id2, Arg.Any<CancellationToken>()).Returns(cat2);

        var command = new ReorderDocumentCategories(new List<ReorderItem>
        {
            new(id1, 2),
            new(id2, 1)
        });

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _repository.Received(2).UpdateAsync(Arg.Any<DocumentCategory>(), Arg.Any<CancellationToken>());
        cat1.SortOrder.Should().Be(2);
        cat2.SortOrder.Should().Be(1);
    }

    [Fact]
    public async Task HandleAsync_ThrowsNotFoundException_WhenCategoryDoesNotExist()
    {
        // Arrange
        var missingId = Guid.NewGuid();
        _repository.GetByIdAsync(missingId, Arg.Any<CancellationToken>()).Returns((DocumentCategory?)null);

        var command = new ReorderDocumentCategories(new List<ReorderItem>
        {
            new(missingId, 1)
        });

        // Act
        var act = () => _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"*{missingId}*");
    }
}
