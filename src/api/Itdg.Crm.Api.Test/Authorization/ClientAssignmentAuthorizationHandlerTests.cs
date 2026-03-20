namespace Itdg.Crm.Api.Test.Authorization;

using System.Security.Claims;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Domain.Repositories;
using Itdg.Crm.Api.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

public class ClientAssignmentAuthorizationHandlerTests
{
    private readonly IUserRepository _userRepository;
    private readonly IClientAssignmentRepository _clientAssignmentRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ClientAssignmentAuthorizationHandler> _logger;
    private readonly ClientAssignmentAuthorizationHandler _handler;

    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _clientId = Guid.NewGuid();
    private const string EntraObjectId = "00000000-0000-0000-0000-000000000001";

    public ClientAssignmentAuthorizationHandlerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _clientAssignmentRepository = Substitute.For<IClientAssignmentRepository>();
        _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        _logger = Substitute.For<ILogger<ClientAssignmentAuthorizationHandler>>();
        _handler = new ClientAssignmentAuthorizationHandler(
            _userRepository,
            _clientAssignmentRepository,
            _httpContextAccessor,
            _logger);
    }

    [Fact]
    public async Task HandleRequirementAsync_Administrator_SucceedsWithoutAssignmentCheck()
    {
        // Arrange
        var user = CreateUser(UserRole.Administrator);
        _userRepository.GetByEntraObjectIdAsync(EntraObjectId, Arg.Any<CancellationToken>())
            .Returns(user);

        var httpContext = CreateHttpContext(EntraObjectId, _clientId);
        _httpContextAccessor.HttpContext.Returns(httpContext);

        var context = CreateAuthorizationHandlerContext(EntraObjectId);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
        await _clientAssignmentRepository.DidNotReceive()
            .ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleRequirementAsync_AssociateAssigned_Succeeds()
    {
        // Arrange
        var user = CreateUser(UserRole.Associate);
        _userRepository.GetByEntraObjectIdAsync(EntraObjectId, Arg.Any<CancellationToken>())
            .Returns(user);
        _clientAssignmentRepository.ExistsAsync(_userId, _clientId, Arg.Any<CancellationToken>())
            .Returns(true);

        var httpContext = CreateHttpContext(EntraObjectId, _clientId);
        _httpContextAccessor.HttpContext.Returns(httpContext);

        var context = CreateAuthorizationHandlerContext(EntraObjectId);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_AssociateNotAssigned_DoesNotSucceed()
    {
        // Arrange
        var user = CreateUser(UserRole.Associate);
        _userRepository.GetByEntraObjectIdAsync(EntraObjectId, Arg.Any<CancellationToken>())
            .Returns(user);
        _clientAssignmentRepository.ExistsAsync(_userId, _clientId, Arg.Any<CancellationToken>())
            .Returns(false);

        var httpContext = CreateHttpContext(EntraObjectId, _clientId);
        _httpContextAccessor.HttpContext.Returns(httpContext);

        var context = CreateAuthorizationHandlerContext(EntraObjectId);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_ClientPortalAssigned_Succeeds()
    {
        // Arrange
        var user = CreateUser(UserRole.ClientPortal);
        _userRepository.GetByEntraObjectIdAsync(EntraObjectId, Arg.Any<CancellationToken>())
            .Returns(user);
        _clientAssignmentRepository.ExistsAsync(_userId, _clientId, Arg.Any<CancellationToken>())
            .Returns(true);

        var httpContext = CreateHttpContext(EntraObjectId, _clientId);
        _httpContextAccessor.HttpContext.Returns(httpContext);

        var context = CreateAuthorizationHandlerContext(EntraObjectId);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_ClientPortalNotAssigned_DoesNotSucceed()
    {
        // Arrange
        var user = CreateUser(UserRole.ClientPortal);
        _userRepository.GetByEntraObjectIdAsync(EntraObjectId, Arg.Any<CancellationToken>())
            .Returns(user);
        _clientAssignmentRepository.ExistsAsync(_userId, _clientId, Arg.Any<CancellationToken>())
            .Returns(false);

        var httpContext = CreateHttpContext(EntraObjectId, _clientId);
        _httpContextAccessor.HttpContext.Returns(httpContext);

        var context = CreateAuthorizationHandlerContext(EntraObjectId);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_NoHttpContext_DoesNotSucceed()
    {
        // Arrange
        _httpContextAccessor.HttpContext.Returns((HttpContext?)null);

        var context = CreateAuthorizationHandlerContext(EntraObjectId);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_NoEntraObjectIdClaim_DoesNotSucceed()
    {
        // Arrange
        var httpContext = CreateHttpContext(null, _clientId);
        _httpContextAccessor.HttpContext.Returns(httpContext);

        var context = CreateAuthorizationHandlerContext(null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_UserNotFound_DoesNotSucceed()
    {
        // Arrange
        _userRepository.GetByEntraObjectIdAsync(EntraObjectId, Arg.Any<CancellationToken>())
            .Returns((User?)null);

        var httpContext = CreateHttpContext(EntraObjectId, _clientId);
        _httpContextAccessor.HttpContext.Returns(httpContext);

        var context = CreateAuthorizationHandlerContext(EntraObjectId);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_NoClientIdRouteValue_DoesNotSucceed()
    {
        // Arrange
        var user = CreateUser(UserRole.Associate);
        _userRepository.GetByEntraObjectIdAsync(EntraObjectId, Arg.Any<CancellationToken>())
            .Returns(user);

        var httpContext = CreateHttpContext(EntraObjectId, clientId: null);
        _httpContextAccessor.HttpContext.Returns(httpContext);

        var context = CreateAuthorizationHandlerContext(EntraObjectId);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }

    [Fact]
    public async Task HandleRequirementAsync_UsesOidClaimAsFallback()
    {
        // Arrange
        var user = CreateUser(UserRole.Administrator);
        _userRepository.GetByEntraObjectIdAsync(EntraObjectId, Arg.Any<CancellationToken>())
            .Returns(user);

        var httpContext = CreateHttpContext(EntraObjectId, _clientId);
        _httpContextAccessor.HttpContext.Returns(httpContext);

        // Use "oid" claim instead of full claim type
        var claims = new[] { new Claim("oid", EntraObjectId) };
        var identity = new ClaimsIdentity(claims, "TestScheme");
        var principal = new ClaimsPrincipal(identity);

        var requirement = new ClientAssignmentRequirement();
        var context = new AuthorizationHandlerContext(
            new[] { requirement },
            principal,
            null);

        // Act
        await _handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    private User CreateUser(UserRole role)
    {
        return new User
        {
            Id = _userId,
            TenantId = _tenantId,
            EntraObjectId = EntraObjectId,
            Email = "test@example.com",
            DisplayName = "Test User",
            Role = role,
            IsActive = true
        };
    }

    private static HttpContext CreateHttpContext(string? entraObjectId, Guid? clientId)
    {
        var httpContext = new DefaultHttpContext();

        var routeValues = new RouteValueDictionary();
        if (clientId.HasValue)
        {
            routeValues["client_id"] = clientId.Value.ToString();
        }
        httpContext.Request.RouteValues = routeValues;

        return httpContext;
    }

    private static AuthorizationHandlerContext CreateAuthorizationHandlerContext(string? entraObjectId)
    {
        var claims = new List<Claim>();
        if (entraObjectId is not null)
        {
            claims.Add(new Claim("http://schemas.microsoft.com/identity/claims/objectidentifier", entraObjectId));
        }

        var identity = new ClaimsIdentity(claims, entraObjectId is not null ? "TestScheme" : null);
        var principal = new ClaimsPrincipal(identity);

        var requirement = new ClientAssignmentRequirement();
        return new AuthorizationHandlerContext(
            new[] { requirement },
            principal,
            null);
    }
}
