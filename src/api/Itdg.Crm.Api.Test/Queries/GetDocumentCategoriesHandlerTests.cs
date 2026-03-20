namespace Itdg.Crm.Api.Test.Queries;

using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Application.QueryHandlers;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetDocumentCategoriesHandlerTests
{
    private readonly IGenericRepository<DocumentCategory> _repository;
    private readonly ILogger<GetDocumentCategoriesHandler> _logger;
    private readonly GetDocumentCategoriesHandler _handler;

    public GetDocumentCategoriesHandlerTests()
    {
        _repository = Substitute.For<IGenericRepository<DocumentCategory>>();
        _logger = Substitute.For<ILogger<GetDocumentCategoriesHandler>>();
        _handler = new GetDocumentCategoriesHandler(_repository, _logger);
    }

    [Fact]
    public async Task HandleAsync_ReturnsAllCategories_OrderedBySortOrder()
    {
        // Arrange
        var categories = new List<DocumentCategory>
        {
            new()
            {
                Id = Guid.NewGuid(), TenantId = Guid.NewGuid(),
                Name = "Cat C", SortOrder = 3, IsDefault = false,
                CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(), TenantId = Guid.NewGuid(),
                Name = "Cat A", SortOrder = 1, IsDefault = true,
                CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(), TenantId = Guid.NewGuid(),
                Name = "Cat B", SortOrder = 2, IsDefault = false,
                CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow
            }
        };

        _repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(categories.AsReadOnly());

        // Act
        var result = await _handler.HandleAsync(new GetDocumentCategories(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        var dtos = result.ToList();
        dtos.Should().HaveCount(3);
        dtos[0].Name.Should().Be("Cat A");
        dtos[1].Name.Should().Be("Cat B");
        dtos[2].Name.Should().Be("Cat C");
    }

    [Fact]
    public async Task HandleAsync_ReturnsEmpty_WhenNoCategoriesExist()
    {
        // Arrange
        _repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<DocumentCategory>().AsReadOnly());

        // Act
        var result = await _handler.HandleAsync(new GetDocumentCategories(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_MapsPropertiesCorrectly()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow.AddDays(-1);
        var updatedAt = DateTimeOffset.UtcNow;

        var categories = new List<DocumentCategory>
        {
            new()
            {
                Id = categoryId, TenantId = Guid.NewGuid(),
                Name = "Tax Documents",
                NamingConvention = "{ClientName}_TaxDoc_{Date}",
                SortOrder = 1, IsDefault = true,
                CreatedAt = createdAt, UpdatedAt = updatedAt
            }
        };

        _repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(categories.AsReadOnly());

        // Act
        var result = await _handler.HandleAsync(new GetDocumentCategories(), Guid.NewGuid(), CancellationToken.None);

        // Assert
        var dto = result.Single();
        dto.CategoryId.Should().Be(categoryId);
        dto.Name.Should().Be("Tax Documents");
        dto.NamingConvention.Should().Be("{ClientName}_TaxDoc_{Date}");
        dto.IsDefault.Should().BeTrue();
        dto.SortOrder.Should().Be(1);
        dto.CreatedAt.Should().Be(createdAt);
        dto.UpdatedAt.Should().Be(updatedAt);
    }
}
