namespace Itdg.Crm.Api.Infrastructure.Services;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Infrastructure.Options;
using Microsoft.Extensions.Options;

public class PortalConfiguration : IPortalConfiguration
{
    private readonly PortalOptions _options;

    public PortalConfiguration(IOptions<PortalOptions> options)
    {
        _options = options.Value;
    }

    public string GetBaseUrl() => _options.BaseUrl;

    public int GetInvitationExpiryDays() => _options.InvitationExpiryDays;
}
