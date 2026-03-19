namespace Itdg.Crm.Api.Test.Repositories;

using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Infrastructure.Repositories;

public class ClientRepositoryTests
{
    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
        {
        }

        public DbSet<Client> Clients => Set<Client>();
        public DbSet<ClientTier> ClientTiers => Set<ClientTier>();
    }

    private TestDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new TestDbContext(options);
    }

    private class TestClientRepository : GenericRepository<Client>
    {
        public TestClientRepository(TestDbContext context) : base(context)
        {
        }
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsClient_WhenClientExists()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new TestClientRepository(context);

        var tier = new ClientTier { Id = Guid.NewGuid(), Name = "Tier 1", SortOrder = 1, TenantId = Guid.NewGuid() };
        await context.ClientTiers.AddAsync(tier);

        var client = new Client
        {
            Id = Guid.NewGuid(),
            Name = "Test Client",
            ContactEmail = "test@example.com",
            Phone = "787-555-1234",
            Address = "123 Main St",
            TierId = tier.Id,
            Status = ClientStatus.Active,
            IndustryTag = "Technology",
            Notes = "Test notes",
            CustomFields = "{\"key\":\"value\"}",
            TenantId = Guid.NewGuid()
        };
        await context.Clients.AddAsync(client);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetByIdAsync(client.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(client.Id);
        result.Name.Should().Be("Test Client");
        result.ContactEmail.Should().Be("test@example.com");
        result.Phone.Should().Be("787-555-1234");
        result.Address.Should().Be("123 Main St");
        result.TierId.Should().Be(tier.Id);
        result.Status.Should().Be(ClientStatus.Active);
        result.IndustryTag.Should().Be("Technology");
        result.Notes.Should().Be("Test notes");
        result.CustomFields.Should().Be("{\"key\":\"value\"}");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenClientDoesNotExist()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new TestClientRepository(context);

        // Act
        var result = await repository.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task AddAsync_AddsClient_AndSavesChanges()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new TestClientRepository(context);

        var client = new Client
        {
            Id = Guid.NewGuid(),
            Name = "New Client",
            Status = ClientStatus.Active,
            TenantId = Guid.NewGuid()
        };

        // Act
        await repository.AddAsync(client);

        // Assert
        var savedClient = await context.Clients.FindAsync(client.Id);
        savedClient.Should().NotBeNull();
        savedClient!.Name.Should().Be("New Client");
        savedClient.Status.Should().Be(ClientStatus.Active);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesClient_AndSavesChanges()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new TestClientRepository(context);

        var client = new Client
        {
            Id = Guid.NewGuid(),
            Name = "Original Name",
            Status = ClientStatus.Active,
            TenantId = Guid.NewGuid()
        };
        await context.Clients.AddAsync(client);
        await context.SaveChangesAsync();

        // Act
        client.Name = "Updated Name";
        client.Status = ClientStatus.Inactive;
        await repository.UpdateAsync(client);

        // Assert
        var updatedClient = await context.Clients.FindAsync(client.Id);
        updatedClient.Should().NotBeNull();
        updatedClient!.Name.Should().Be("Updated Name");
        updatedClient.Status.Should().Be(ClientStatus.Inactive);
    }

    [Fact]
    public async Task DeleteAsync_RemovesClient_AndSavesChanges()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new TestClientRepository(context);

        var client = new Client
        {
            Id = Guid.NewGuid(),
            Name = "To Be Deleted",
            Status = ClientStatus.Active,
            TenantId = Guid.NewGuid()
        };
        await context.Clients.AddAsync(client);
        await context.SaveChangesAsync();

        // Act
        await repository.DeleteAsync(client);

        // Assert
        var deletedClient = await context.Clients.FindAsync(client.Id);
        deletedClient.Should().BeNull();
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllClients()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new TestClientRepository(context);
        var tenantId = Guid.NewGuid();

        var clients = new[]
        {
            new Client { Id = Guid.NewGuid(), Name = "Client 1", Status = ClientStatus.Active, TenantId = tenantId },
            new Client { Id = Guid.NewGuid(), Name = "Client 2", Status = ClientStatus.Inactive, TenantId = tenantId },
            new Client { Id = Guid.NewGuid(), Name = "Client 3", Status = ClientStatus.Suspended, TenantId = tenantId }
        };

        await context.Clients.AddRangeAsync(clients);
        await context.SaveChangesAsync();

        // Act
        var result = await repository.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(c => c.Name == "Client 1");
        result.Should().Contain(c => c.Name == "Client 2");
        result.Should().Contain(c => c.Name == "Client 3");
    }

    [Fact]
    public async Task Client_CanHaveNullOptionalFields()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var repository = new TestClientRepository(context);

        var client = new Client
        {
            Id = Guid.NewGuid(),
            Name = "Minimal Client",
            Status = ClientStatus.Active,
            TenantId = Guid.NewGuid()
        };

        // Act
        await repository.AddAsync(client);
        var result = await repository.GetByIdAsync(client.Id);

        // Assert
        result.Should().NotBeNull();
        result!.ContactEmail.Should().BeNull();
        result.Phone.Should().BeNull();
        result.Address.Should().BeNull();
        result.TierId.Should().BeNull();
        result.IndustryTag.Should().BeNull();
        result.Notes.Should().BeNull();
        result.CustomFields.Should().BeNull();
        result.DeletedAt.Should().BeNull();
    }

    [Fact]
    public void ClientStatus_HasExpectedValues()
    {
        // Assert
        Enum.GetValues<ClientStatus>().Should().HaveCount(3);
        ClientStatus.Active.Should().BeDefined();
        ClientStatus.Inactive.Should().BeDefined();
        ClientStatus.Suspended.Should().BeDefined();
    }

    [Fact]
    public void ClientTier_HasRequiredProperties()
    {
        // Arrange & Act
        var tier = new ClientTier
        {
            Id = Guid.NewGuid(),
            Name = "Premium",
            SortOrder = 1,
            TenantId = Guid.NewGuid()
        };

        // Assert
        tier.Name.Should().Be("Premium");
        tier.SortOrder.Should().Be(1);
        tier.TenantId.Should().NotBeEmpty();
    }
}
