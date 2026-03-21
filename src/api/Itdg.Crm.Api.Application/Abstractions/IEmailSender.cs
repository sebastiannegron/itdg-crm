namespace Itdg.Crm.Api.Application.Abstractions;

public interface IEmailSender
{
    /// <summary>
    /// Sends a transactional email with a plain-text body.
    /// </summary>
    Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a template-based email with an HTML body.
    /// </summary>
    Task SendHtmlAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default);
}
