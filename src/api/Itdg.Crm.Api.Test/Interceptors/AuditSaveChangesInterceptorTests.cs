namespace Itdg.Crm.Api.Test.Interceptors;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Infrastructure.Interceptors;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

public class AuditSaveChangesInterceptorTests
{
    private class TestTenantEntity : TenantEntity
    {
        public required string Name { get; set; }
    }

    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }

        public DbSet<TestTenantEntity> TestEntities => Set<TestTenantEntity>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    }

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<AuditSaveChangesInterceptor> _logger;
    private readonly Guid _testUserId = Guid.NewGuid();
    private readonly Guid _testTenantId = Guid.NewGuid();

    public AuditSaveChangesInterceptorTests()
    {
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _tenantProvider = Substitute.For<ITenantProvider>();
        _logger = Substitute.For<ILogger<AuditSaveChangesInterceptor>>();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, _testUserId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = principal };
        _httpContextAccessor.HttpContext.Returns(httpContext);
        _tenantProvider.GetTenantId().Returns(_testTenantId);
    }

    private TestDbContext CreateContext()
    {
        var interceptor = new AuditSaveChangesInterceptor(
            _httpContextAccessor,
            _tenantProvider,
            _logger);

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .AddInterceptors(interceptor)
            .Options;

        return new TestDbContext(options);
    }

    [Fact]
    public async Task SavingChanges_CreatesAuditLog_WhenEntityAdded()
    {
        // Arrange
        using var context = CreateContext();
        var entity = new TestTenantEntity
        {
            Id = Guid.NewGuid(),
            TenantId = _testTenantId,
            Name = "New Entity"
        };

        // Act
        context.TestEntities.Add(entity);
        await context.SaveChangesAsync();

        // Assert
        var auditLogs = context.AuditLogs.ToList();
        auditLogs.Should().HaveCount(1);
        auditLogs[0].EntityType.Should().Be(nameof(TestTenantEntity));
        auditLogs[0].EntityId.Should().Be(entity.Id);
        auditLogs[0].Action.Should().Be("Added");
        auditLogs[0].OldValues.Should().BeNull();
        auditLogs[0].NewValues.Should().NotBeNull();
        auditLogs[0].UserId.Should().Be(_testUserId);
        auditLogs[0].TenantId.Should().Be(_testTenantId);
    }

    [Fact]
    public async Task SavingChanges_CreatesAuditLog_WhenEntityModified()
    {
        // Arrange
        using var context = CreateContext();
        var entity = new TestTenantEntity
        {
            Id = Guid.NewGuid(),
            TenantId = _testTenantId,
            Name = "Original"
        };
        context.TestEntities.Add(entity);
        await context.SaveChangesAsync();

        // Clear existing audit logs from add operation
        context.AuditLogs.RemoveRange(context.AuditLogs);
        await context.SaveChangesAsync(CancellationToken.None);

        // Act
        entity.Name = "Updated";
        context.TestEntities.Update(entity);
        await context.SaveChangesAsync();

        // Assert
        var auditLogs = context.AuditLogs.ToList();
        auditLogs.Should().HaveCount(1);
        auditLogs[0].Action.Should().Be("Modified");
        auditLogs[0].OldValues.Should().NotBeNull();
        auditLogs[0].NewValues.Should().NotBeNull();
        auditLogs[0].OldValues.Should().Contain("Original");
        auditLogs[0].NewValues.Should().Contain("Updated");
    }

    [Fact]
    public async Task SavingChanges_CreatesAuditLog_WhenEntityDeleted()
    {
        // Arrange
        using var context = CreateContext();
        var entity = new TestTenantEntity
        {
            Id = Guid.NewGuid(),
            TenantId = _testTenantId,
            Name = "To Delete"
        };
        context.TestEntities.Add(entity);
        await context.SaveChangesAsync();

        // Clear existing audit logs from add operation
        context.AuditLogs.RemoveRange(context.AuditLogs);
        await context.SaveChangesAsync(CancellationToken.None);

        // Act
        context.TestEntities.Remove(entity);
        await context.SaveChangesAsync();

        // Assert
        var auditLogs = context.AuditLogs.ToList();
        auditLogs.Should().HaveCount(1);
        auditLogs[0].Action.Should().Be("Deleted");
        auditLogs[0].OldValues.Should().NotBeNull();
        auditLogs[0].NewValues.Should().BeNull();
    }

    [Fact]
    public async Task SavingChanges_DoesNotAuditAuditLogEntities()
    {
        // Arrange
        using var context = CreateContext();
        var auditLog = new AuditLog
        {
            Id = Guid.NewGuid(),
            TenantId = _testTenantId,
            UserId = _testUserId,
            EntityType = "TestEntity",
            EntityId = Guid.NewGuid(),
            Action = "Added",
            Timestamp = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        // Act
        context.AuditLogs.Add(auditLog);
        await context.SaveChangesAsync();

        // Assert — only the manually added audit log should exist, no additional audit of the audit
        var auditLogs = context.AuditLogs.ToList();
        auditLogs.Should().HaveCount(1);
        auditLogs[0].Id.Should().Be(auditLog.Id);
    }

    [Fact]
    public async Task SavingChanges_SetsUserIdToEmpty_WhenNoHttpContext()
    {
        // Arrange
        _httpContextAccessor.HttpContext.Returns((HttpContext?)null);
        using var context = CreateContext();
        var entity = new TestTenantEntity
        {
            Id = Guid.NewGuid(),
            TenantId = _testTenantId,
            Name = "No User"
        };

        // Act
        context.TestEntities.Add(entity);
        await context.SaveChangesAsync();

        // Assert
        var auditLogs = context.AuditLogs.ToList();
        auditLogs.Should().HaveCount(1);
        auditLogs[0].UserId.Should().Be(Guid.Empty);
    }

    [Fact]
    public async Task SavingChanges_SetsTimestamp_ToUtcNow()
    {
        // Arrange
        using var context = CreateContext();
        var entity = new TestTenantEntity
        {
            Id = Guid.NewGuid(),
            TenantId = _testTenantId,
            Name = "Timestamp Test"
        };
        var before = DateTimeOffset.UtcNow;

        // Act
        context.TestEntities.Add(entity);
        await context.SaveChangesAsync();

        // Assert
        var after = DateTimeOffset.UtcNow;
        var auditLogs = context.AuditLogs.ToList();
        auditLogs.Should().HaveCount(1);
        auditLogs[0].Timestamp.Should().BeOnOrAfter(before);
        auditLogs[0].Timestamp.Should().BeOnOrBefore(after);
    }
}
