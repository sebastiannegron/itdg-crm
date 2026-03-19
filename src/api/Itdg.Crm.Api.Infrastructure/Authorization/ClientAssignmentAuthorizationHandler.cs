namespace Itdg.Crm.Api.Infrastructure.Authorization;

using System.Security.Claims;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

public class ClientAssignmentAuthorizationHandler : AuthorizationHandler<ClientAssignmentRequirement>
{
    private readonly IUserRepository _userRepository;
    private readonly IClientAssignmentRepository _clientAssignmentRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ClientAssignmentAuthorizationHandler> _logger;

    public ClientAssignmentAuthorizationHandler(
        IUserRepository userRepository,
        IClientAssignmentRepository clientAssignmentRepository,
        IHttpContextAccessor httpContextAccessor,
        ILogger<ClientAssignmentAuthorizationHandler> logger)
    {
        _userRepository = userRepository;
        _clientAssignmentRepository = clientAssignmentRepository;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ClientAssignmentRequirement requirement)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            _logger.LogWarning("HttpContext is not available for client assignment authorization");
            return;
        }

        var entraObjectId = context.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value
            ?? context.User.FindFirst("oid")?.Value;

        if (string.IsNullOrWhiteSpace(entraObjectId))
        {
            _logger.LogWarning("EntraObjectId claim is missing from the authenticated user");
            return;
        }

        var user = await _userRepository.GetByEntraObjectIdAsync(entraObjectId, httpContext.RequestAborted);
        if (user is null)
        {
            _logger.LogWarning("User not found for EntraObjectId {EntraObjectId}", entraObjectId);
            return;
        }

        // Administrators bypass client assignment checks
        if (user.Role == UserRole.Administrator)
        {
            context.Succeed(requirement);
            return;
        }

        // Extract client_id from route values
        var clientIdRouteValue = httpContext.Request.RouteValues["client_id"]?.ToString()
            ?? httpContext.Request.RouteValues["id"]?.ToString();

        if (string.IsNullOrWhiteSpace(clientIdRouteValue) || !Guid.TryParse(clientIdRouteValue, out var clientId))
        {
            _logger.LogWarning("client_id route value is missing or invalid for client assignment authorization");
            return;
        }

        var isAssigned = await _clientAssignmentRepository.ExistsAsync(user.Id, clientId, httpContext.RequestAborted);
        if (isAssigned)
        {
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning(
                "User {UserId} is not assigned to client {ClientId}",
                user.Id, clientId);
        }
    }
}
