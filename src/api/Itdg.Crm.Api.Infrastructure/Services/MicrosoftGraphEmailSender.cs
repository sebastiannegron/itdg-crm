namespace Itdg.Crm.Api.Infrastructure.Services;

using System.Diagnostics;
using Azure.Identity;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Graph.Users.Item.SendMail;

public class MicrosoftGraphEmailSender : IEmailSender
{
    private readonly MicrosoftGraphEmailOptions _options;
    private readonly ILogger<MicrosoftGraphEmailSender> _logger;
    private readonly GraphServiceClient _graphClient;

    public MicrosoftGraphEmailSender(
        IOptions<MicrosoftGraphEmailOptions> options,
        ILogger<MicrosoftGraphEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
        _graphClient = CreateGraphClient();
    }

    internal MicrosoftGraphEmailSender(
        MicrosoftGraphEmailOptions options,
        ILogger<MicrosoftGraphEmailSender> logger,
        GraphServiceClient graphClient)
    {
        _options = options;
        _logger = logger;
        _graphClient = graphClient;
    }

    public async Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Send Email");
        activity?.SetTag("ToEmail", toEmail);
        activity?.SetTag("Subject", subject);
        activity?.SetTag("ContentType", "Text");

        _logger.LogInformation("Sending text email to {ToEmail} with subject '{Subject}'", toEmail, subject);

        var message = BuildMessage(toEmail, subject, body, BodyType.Text);

        await SendMailAsync(message, cancellationToken);

        _logger.LogInformation("Text email sent successfully to {ToEmail}", toEmail);
    }

    public async Task SendHtmlAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Send HTML Email");
        activity?.SetTag("ToEmail", toEmail);
        activity?.SetTag("Subject", subject);
        activity?.SetTag("ContentType", "Html");

        _logger.LogInformation("Sending HTML email to {ToEmail} with subject '{Subject}'", toEmail, subject);

        var message = BuildMessage(toEmail, subject, htmlBody, BodyType.Html);

        await SendMailAsync(message, cancellationToken);

        _logger.LogInformation("HTML email sent successfully to {ToEmail}", toEmail);
    }

    internal static Message BuildMessage(string toEmail, string subject, string content, BodyType bodyType)
    {
        return new Message
        {
            Subject = subject,
            Body = new ItemBody
            {
                ContentType = bodyType,
                Content = content
            },
            ToRecipients =
            [
                new Recipient
                {
                    EmailAddress = new EmailAddress
                    {
                        Address = toEmail
                    }
                }
            ]
        };
    }

    private async Task SendMailAsync(Message message, CancellationToken cancellationToken)
    {
        var requestBody = new SendMailPostRequestBody
        {
            Message = message,
            SaveToSentItems = false
        };

        await _graphClient.Users[_options.SenderAddress]
            .SendMail
            .PostAsync(requestBody, cancellationToken: cancellationToken);
    }

    private GraphServiceClient CreateGraphClient()
    {
        var credential = new ClientSecretCredential(
            _options.TenantId,
            _options.ClientId,
            _options.ClientSecret);

        return new GraphServiceClient(credential, ["https://graph.microsoft.com/.default"]);
    }
}
