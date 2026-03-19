namespace Itdg.Crm.Api.Test.Repositories;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.Enums;
using Itdg.Crm.Api.Infrastructure.Data;
using Itdg.Crm.Api.Infrastructure.Repositories;

public class TemplateRepositoryTests
{
    private class TestCrmDbContext : CrmDbContext
    {
        public TestCrmDbContext(DbContextOptions<TestCrmDbContext> options, ITenantProvider tenantProvider)
            : base(options, tenantProvider)
        {
        }
    }

    private static TestCrmDbContext CreateContext(Guid tenantId, string databaseName)
    {
        var tenantProvider = Substitute.For<ITenantProvider>();
        tenantProvider.GetTenantId().Returns(tenantId);

        var options = new DbContextOptionsBuilder<TestCrmDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new TestCrmDbContext(options, tenantProvider);
    }

    private static CommunicationTemplate CreateTemplate(
        Guid tenantId,
        TemplateCategory category = TemplateCategory.General,
        string name = "Test Template",
        string language = "en-pr",
        bool isActive = true)
    {
        return new CommunicationTemplate
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            Category = category,
            Name = name,
            SubjectTemplate = "Subject for {{client_name}}",
            BodyTemplate = "Hello {{client_name}}, this is a test.",
            Language = language,
            Version = 1,
            IsActive = isActive,
            CreatedById = Guid.NewGuid()
        };
    }

    [Fact]
    public async Task GetByCategoryAsync_ReturnsTemplatesForCategory()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        using (var seedContext = CreateContext(tenantId, dbName))
        {
            seedContext.CommunicationTemplates.AddRange(
                CreateTemplate(tenantId, TemplateCategory.Onboarding, "Onboarding 1"),
                CreateTemplate(tenantId, TemplateCategory.Onboarding, "Onboarding 2"),
                CreateTemplate(tenantId, TemplateCategory.PaymentReminder, "Payment 1")
            );
            await seedContext.SaveChangesAsync();
        }

        // Act
        using var queryContext = CreateContext(tenantId, dbName);
        var repository = new TemplateRepository(queryContext);
        var results = await repository.GetByCategoryAsync(TemplateCategory.Onboarding);

        // Assert
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(t => t.Category.Should().Be(TemplateCategory.Onboarding));
    }

    [Fact]
    public async Task GetByCategoryAsync_ReturnsEmpty_WhenNoneMatchCategory()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        using (var seedContext = CreateContext(tenantId, dbName))
        {
            seedContext.CommunicationTemplates.Add(
                CreateTemplate(tenantId, TemplateCategory.General, "General Template")
            );
            await seedContext.SaveChangesAsync();
        }

        // Act
        using var queryContext = CreateContext(tenantId, dbName);
        var repository = new TemplateRepository(queryContext);
        var results = await repository.GetByCategoryAsync(TemplateCategory.TaxSeason);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task GetActiveAsync_ReturnsOnlyActiveTemplates()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        using (var seedContext = CreateContext(tenantId, dbName))
        {
            seedContext.CommunicationTemplates.AddRange(
                CreateTemplate(tenantId, name: "Active 1", isActive: true),
                CreateTemplate(tenantId, name: "Active 2", isActive: true),
                CreateTemplate(tenantId, name: "Inactive", isActive: false)
            );
            await seedContext.SaveChangesAsync();
        }

        // Act
        using var queryContext = CreateContext(tenantId, dbName);
        var repository = new TemplateRepository(queryContext);
        var results = await repository.GetActiveAsync();

        // Assert
        results.Should().HaveCount(2);
        results.Should().AllSatisfy(t => t.IsActive.Should().BeTrue());
    }

    [Fact]
    public async Task GetActiveAsync_ReturnsEmpty_WhenNoActiveTemplates()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        using (var seedContext = CreateContext(tenantId, dbName))
        {
            seedContext.CommunicationTemplates.Add(
                CreateTemplate(tenantId, name: "Inactive", isActive: false)
            );
            await seedContext.SaveChangesAsync();
        }

        // Act
        using var queryContext = CreateContext(tenantId, dbName);
        var repository = new TemplateRepository(queryContext);
        var results = await repository.GetActiveAsync();

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public async Task TenantFilter_IsolatesTemplatesByTenant()
    {
        // Arrange
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        using (var seedContext = CreateContext(tenantA, dbName))
        {
            seedContext.CommunicationTemplates.AddRange(
                CreateTemplate(tenantA, name: "Tenant A Template"),
                CreateTemplate(tenantB, name: "Tenant B Template")
            );
            await seedContext.SaveChangesAsync();
        }

        // Act
        using var queryContext = CreateContext(tenantA, dbName);
        var repository = new TemplateRepository(queryContext);
        var results = await repository.GetAllAsync();

        // Assert
        results.Should().HaveCount(1);
        results[0].Name.Should().Be("Tenant A Template");
    }

    [Fact]
    public async Task SoftDeleteFilter_ExcludesDeletedTemplates()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var dbName = Guid.NewGuid().ToString();

        using (var seedContext = CreateContext(tenantId, dbName))
        {
            var active = CreateTemplate(tenantId, name: "Active");
            var deleted = CreateTemplate(tenantId, name: "Deleted");
            deleted.DeletedAt = DateTimeOffset.UtcNow;

            seedContext.CommunicationTemplates.AddRange(active, deleted);
            await seedContext.SaveChangesAsync();
        }

        // Act
        using var queryContext = CreateContext(tenantId, dbName);
        var repository = new TemplateRepository(queryContext);
        var results = await repository.GetAllAsync();

        // Assert
        results.Should().HaveCount(1);
        results[0].Name.Should().Be("Active");
    }
}
