namespace Itdg.Crm.Api.Test.Endpoints;

using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;

public class HealthEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public HealthEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove the real DbContext registration for testing
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<Itdg.Crm.Api.Infrastructure.Data.CrmDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database
                services.AddDbContext<Itdg.Crm.Api.Infrastructure.Data.CrmDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb"));
            });
        }).CreateClient();
    }

    [Fact]
    public async Task GetHealth_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/Health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
