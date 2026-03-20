namespace Itdg.Crm.Api.Infrastructure.Services;

using Microsoft.Extensions.Logging;

public class NoOpEmailSender : IEmailSender
{
    private readonly ILogger<NoOpEmailSender> _logger;

    public NoOpEmailSender(ILogger<NoOpEmailSender> logger)
    {
        _logger = logger;
    }

    public Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Email sending not configured. Would send to {ToEmail} with subject '{Subject}'", toEmail, subject);
        return Task.CompletedTask;
    }
}
