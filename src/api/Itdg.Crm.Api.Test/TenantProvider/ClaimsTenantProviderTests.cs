namespace Itdg.Crm.Api.Test.TenantProvider;

using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Itdg.Crm.Api.Infrastructure.TenantProvider;

public class ClaimsTenantProviderTests
{
    [Fact]
    public void GetTenantId_ReturnsValidTenantId_WhenClaimExists()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var claims = new[]
        {
            new Claim("TenantId", tenantId.ToString()),
            new Claim(ClaimTypes.Name, "testuser@example.com")
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = claimsPrincipal };
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns(httpContext);

        var provider = new ClaimsTenantProvider(httpContextAccessor);

        // Act
        var result = provider.GetTenantId();

        // Assert
        result.Should().Be(tenantId);
    }

    [Fact]
    public void GetTenantId_ThrowsInvalidOperationException_WhenHttpContextIsNull()
    {
        // Arrange
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns((HttpContext?)null);

        var provider = new ClaimsTenantProvider(httpContextAccessor);

        // Act
        var act = () => provider.GetTenantId();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("No HTTP context available.");
    }

    [Fact]
    public void GetTenantId_ThrowsInvalidOperationException_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var identity = new ClaimsIdentity(); // Not authenticated
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = claimsPrincipal };
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns(httpContext);

        var provider = new ClaimsTenantProvider(httpContextAccessor);

        // Act
        var act = () => provider.GetTenantId();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("User is not authenticated.");
    }

    [Fact]
    public void GetTenantId_ThrowsUnauthorizedAccessException_WhenTenantIdClaimIsMissing()
    {
        // Arrange
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "testuser@example.com")
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = claimsPrincipal };
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns(httpContext);

        var provider = new ClaimsTenantProvider(httpContextAccessor);

        // Act
        var act = () => provider.GetTenantId();

        // Assert
        act.Should().Throw<UnauthorizedAccessException>()
            .WithMessage("TenantId claim is missing from the user's token.");
    }

    [Fact]
    public void GetTenantId_ThrowsUnauthorizedAccessException_WhenTenantIdClaimIsEmpty()
    {
        // Arrange
        var claims = new[]
        {
            new Claim("TenantId", ""),
            new Claim(ClaimTypes.Name, "testuser@example.com")
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = claimsPrincipal };
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns(httpContext);

        var provider = new ClaimsTenantProvider(httpContextAccessor);

        // Act
        var act = () => provider.GetTenantId();

        // Assert
        act.Should().Throw<UnauthorizedAccessException>()
            .WithMessage("TenantId claim is missing from the user's token.");
    }

    [Fact]
    public void GetTenantId_ThrowsUnauthorizedAccessException_WhenTenantIdClaimIsNotValidGuid()
    {
        // Arrange
        var claims = new[]
        {
            new Claim("TenantId", "not-a-guid"),
            new Claim(ClaimTypes.Name, "testuser@example.com")
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var httpContext = new DefaultHttpContext { User = claimsPrincipal };
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        httpContextAccessor.HttpContext.Returns(httpContext);

        var provider = new ClaimsTenantProvider(httpContextAccessor);

        // Act
        var act = () => provider.GetTenantId();

        // Assert
        act.Should().Throw<UnauthorizedAccessException>()
            .WithMessage("TenantId claim is not a valid GUID.");
    }
}
