namespace Itdg.Crm.Api.Test.Middlewares;

using System.Security.Claims;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Domain.Repositories;
using Itdg.Crm.Api.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute.ExceptionExtensions;

public class UserSyncMiddlewareTests
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserSyncMiddleware> _logger;
    private bool _nextCalled;

    public UserSyncMiddlewareTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _logger = Substitute.For<ILogger<UserSyncMiddleware>>();
        _nextCalled = false;
    }

    private UserSyncMiddleware CreateMiddleware()
    {
        return new UserSyncMiddleware(_ =>
        {
            _nextCalled = true;
            return Task.CompletedTask;
        }, _logger);
    }

    private static HttpContext CreateHttpContext(ClaimsPrincipal? principal = null)
    {
        var context = new DefaultHttpContext();
        if (principal is not null)
        {
            context.User = principal;
        }
        return context;
    }

    private static ClaimsPrincipal CreateAuthenticatedPrincipal(
        string entraObjectId,
        string tenantId,
        string? email = null,
        string? displayName = null,
        string? role = null)
    {
        var claims = new List<Claim>
        {
            new("oid", entraObjectId),
            new("TenantId", tenantId)
        };

        if (email is not null)
            claims.Add(new Claim(ClaimTypes.Email, email));

        if (displayName is not null)
            claims.Add(new Claim("name", displayName));

        if (role is not null)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var identity = new ClaimsIdentity(claims, "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    [Fact]
    public async Task InvokeAsync_CallsNext_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var context = CreateHttpContext();

        // Act
        await middleware.InvokeAsync(context, _userRepository);

        // Assert
        _nextCalled.Should().BeTrue();
        await _userRepository.DidNotReceive().GetByEntraObjectIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InvokeAsync_CallsNext_WhenUserAlreadyExists()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var tenantId = Guid.NewGuid();
        var principal = CreateAuthenticatedPrincipal("existing-oid", tenantId.ToString(), "user@example.com");
        var context = CreateHttpContext(principal);

        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EntraObjectId = "existing-oid",
            Email = "user@example.com",
            DisplayName = "Existing User",
            Role = UserRole.Associate,
            IsActive = true
        };
        _userRepository.GetByEntraObjectIdAsync("existing-oid", Arg.Any<CancellationToken>())
            .Returns(existingUser);

        // Act
        await middleware.InvokeAsync(context, _userRepository);

        // Assert
        _nextCalled.Should().BeTrue();
        await _userRepository.Received(1).GetByEntraObjectIdAsync("existing-oid", Arg.Any<CancellationToken>());
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InvokeAsync_CreatesUser_WhenUserDoesNotExist()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var tenantId = Guid.NewGuid();
        var principal = CreateAuthenticatedPrincipal("new-oid", tenantId.ToString(), "new@example.com", "New User", "Administrator");
        var context = CreateHttpContext(principal);

        _userRepository.GetByEntraObjectIdAsync("new-oid", Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        await middleware.InvokeAsync(context, _userRepository);

        // Assert
        _nextCalled.Should().BeTrue();
        await _userRepository.Received(1).AddAsync(
            Arg.Is<User>(u =>
                u.EntraObjectId == "new-oid" &&
                u.TenantId == tenantId &&
                u.Email == "new@example.com" &&
                u.DisplayName == "New User" &&
                u.Role == UserRole.Administrator &&
                u.IsActive),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InvokeAsync_DefaultsRoleToAssociate_WhenRoleClaimMissing()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var tenantId = Guid.NewGuid();
        var principal = CreateAuthenticatedPrincipal("no-role-oid", tenantId.ToString(), "norole@example.com", "No Role User");
        var context = CreateHttpContext(principal);

        _userRepository.GetByEntraObjectIdAsync("no-role-oid", Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        await middleware.InvokeAsync(context, _userRepository);

        // Assert
        await _userRepository.Received(1).AddAsync(
            Arg.Is<User>(u => u.Role == UserRole.Associate),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InvokeAsync_CallsNext_WhenEntraObjectIdClaimIsMissing()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var claims = new List<Claim> { new("TenantId", Guid.NewGuid().ToString()) };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var context = CreateHttpContext(principal);

        // Act
        await middleware.InvokeAsync(context, _userRepository);

        // Assert
        _nextCalled.Should().BeTrue();
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InvokeAsync_CallsNext_WhenTenantIdClaimIsMissing()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var claims = new List<Claim> { new("oid", "some-oid") };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var context = CreateHttpContext(principal);

        _userRepository.GetByEntraObjectIdAsync("some-oid", Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        await middleware.InvokeAsync(context, _userRepository);

        // Assert
        _nextCalled.Should().BeTrue();
        await _userRepository.DidNotReceive().AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task InvokeAsync_CallsNext_EvenWhenAddAsyncThrows()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var tenantId = Guid.NewGuid();
        var principal = CreateAuthenticatedPrincipal("error-oid", tenantId.ToString(), "error@example.com", "Error User");
        var context = CreateHttpContext(principal);

        _userRepository.GetByEntraObjectIdAsync("error-oid", Arg.Any<CancellationToken>())
            .Returns((User?)null);
        _userRepository.AddAsync(Arg.Any<User>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Duplicate key"));

        // Act
        await middleware.InvokeAsync(context, _userRepository);

        // Assert
        _nextCalled.Should().BeTrue();
    }

    [Fact]
    public async Task InvokeAsync_UsesFullObjectIdentifierClaim_ForEntraObjectId()
    {
        // Arrange
        var middleware = CreateMiddleware();
        var tenantId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new("http://schemas.microsoft.com/identity/claims/objectidentifier", "full-oid-claim"),
            new("TenantId", tenantId.ToString()),
            new(ClaimTypes.Email, "fulloid@example.com"),
            new("name", "Full OID User")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        var context = CreateHttpContext(principal);

        _userRepository.GetByEntraObjectIdAsync("full-oid-claim", Arg.Any<CancellationToken>())
            .Returns((User?)null);

        // Act
        await middleware.InvokeAsync(context, _userRepository);

        // Assert
        await _userRepository.Received(1).AddAsync(
            Arg.Is<User>(u => u.EntraObjectId == "full-oid-claim"),
            Arg.Any<CancellationToken>());
    }
}
