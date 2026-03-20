namespace Itdg.Crm.Api.Infrastructure.Services;

using System.Diagnostics;
using System.Net.Mime;
using System.Text;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Gmail.v1.Data;
using Google.Apis.Services;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class GmailService : IGmailService
{
    private readonly GmailOptions _options;
    private readonly ILogger<GmailService> _logger;

    public GmailService(IOptions<GmailOptions> options, ILogger<GmailService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<GmailMessageListDto> ListMessagesAsync(
        string userAccessToken,
        string? query = null,
        int maxResults = 20,
        string? pageToken = null,
        CancellationToken cancellationToken = default)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Gmail List Messages");

        using var service = CreateGoogleGmailClient(userAccessToken);

        var request = service.Users.Messages.List("me");
        request.MaxResults = maxResults;

        if (!string.IsNullOrWhiteSpace(query))
        {
            request.Q = query;
        }

        if (!string.IsNullOrWhiteSpace(pageToken))
        {
            request.PageToken = pageToken;
        }

        _logger.LogInformation("Listing Gmail messages with query '{Query}', maxResults {MaxResults}", query, maxResults);

        var response = await request.ExecuteAsync(cancellationToken);

        if (response.Messages is null || response.Messages.Count == 0)
        {
            return new GmailMessageListDto([], response.NextPageToken, (int)(response.ResultSizeEstimate ?? 0));
        }

        var messages = new List<GmailMessageDto>();

        foreach (var messageSummary in response.Messages)
        {
            var fullMessage = await GetFullMessageAsync(service, messageSummary.Id, cancellationToken);
            messages.Add(fullMessage);
        }

        return new GmailMessageListDto(messages, response.NextPageToken, (int)(response.ResultSizeEstimate ?? 0));
    }

    public async Task<GmailMessageDto> GetMessageAsync(
        string userAccessToken,
        string messageId,
        CancellationToken cancellationToken = default)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Gmail Get Message");
        activity?.SetTag("MessageId", messageId);

        using var service = CreateGoogleGmailClient(userAccessToken);

        _logger.LogInformation("Getting Gmail message {MessageId}", messageId);

        return await GetFullMessageAsync(service, messageId, cancellationToken);
    }

    public async Task<GmailMessageDto> SendMessageAsync(
        string userAccessToken,
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Gmail Send Message");

        using var service = CreateGoogleGmailClient(userAccessToken);

        var rawMessage = CreateRawMessage(to, subject, body);

        var message = new Message { Raw = rawMessage };

        _logger.LogInformation("Sending Gmail message to {To} with subject '{Subject}'", to, subject);

        var sentMessage = await service.Users.Messages.Send(message, "me").ExecuteAsync(cancellationToken);

        return await GetFullMessageAsync(service, sentMessage.Id, cancellationToken);
    }

    internal Google.Apis.Gmail.v1.GmailService CreateGoogleGmailClient(string userAccessToken)
    {
        var credential = GoogleCredential.FromAccessToken(userAccessToken);

        return new Google.Apis.Gmail.v1.GmailService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = _options.ApplicationName,
        });
    }

    private static async Task<GmailMessageDto> GetFullMessageAsync(
        Google.Apis.Gmail.v1.GmailService service,
        string messageId,
        CancellationToken cancellationToken)
    {
        var request = service.Users.Messages.Get("me", messageId);
        request.Format = UsersResource.MessagesResource.GetRequest.FormatEnum.Full;

        var message = await request.ExecuteAsync(cancellationToken);

        return MapToDto(message);
    }

    internal static GmailMessageDto MapToDto(Message message)
    {
        var headers = message.Payload?.Headers ?? [];
        var subject = GetHeaderValue(headers, "Subject");
        var from = GetHeaderValue(headers, "From");
        var to = GetHeaderValue(headers, "To");
        var dateHeader = GetHeaderValue(headers, "Date");

        var date = DateTimeOffset.TryParse(dateHeader, out var parsedDate)
            ? parsedDate
            : DateTimeOffset.UtcNow;

        var bodyPreview = ExtractBodyPreview(message);
        var hasAttachments = HasAttachments(message.Payload);

        return new GmailMessageDto(
            Id: message.Id,
            ThreadId: message.ThreadId,
            Subject: subject,
            From: from,
            To: to,
            Snippet: message.Snippet ?? string.Empty,
            BodyPreview: bodyPreview,
            HasAttachments: hasAttachments,
            Date: date,
            LabelIds: message.LabelIds is not null
                ? message.LabelIds.ToList().AsReadOnly()
                : []
        );
    }

    private static string GetHeaderValue(IList<MessagePartHeader> headers, string name)
    {
        return headers.FirstOrDefault(h =>
            string.Equals(h.Name, name, StringComparison.OrdinalIgnoreCase))?.Value ?? string.Empty;
    }

    private static string ExtractBodyPreview(Message message)
    {
        if (message.Payload is null)
        {
            return string.Empty;
        }

        // Try to find plain text body first
        var textPart = FindPart(message.Payload, MediaTypeNames.Text.Plain);
        if (textPart?.Body?.Data is not null)
        {
            return DecodeBase64Url(textPart.Body.Data);
        }

        // Fall back to HTML body
        var htmlPart = FindPart(message.Payload, MediaTypeNames.Text.Html);
        if (htmlPart?.Body?.Data is not null)
        {
            return DecodeBase64Url(htmlPart.Body.Data);
        }

        // Fall back to top-level body
        if (message.Payload.Body?.Data is not null)
        {
            return DecodeBase64Url(message.Payload.Body.Data);
        }

        return message.Snippet ?? string.Empty;
    }

    private static MessagePart? FindPart(MessagePart part, string mimeType)
    {
        if (string.Equals(part.MimeType, mimeType, StringComparison.OrdinalIgnoreCase))
        {
            return part;
        }

        if (part.Parts is null)
        {
            return null;
        }

        foreach (var subPart in part.Parts)
        {
            var found = FindPart(subPart, mimeType);
            if (found is not null)
            {
                return found;
            }
        }

        return null;
    }

    private static bool HasAttachments(MessagePart? payload)
    {
        if (payload is null)
        {
            return false;
        }

        if (payload.Parts is null)
        {
            return !string.IsNullOrEmpty(payload.Filename);
        }

        return payload.Parts.Any(p => !string.IsNullOrEmpty(p.Filename) || HasAttachments(p));
    }

    internal static string DecodeBase64Url(string base64Url)
    {
        var base64 = base64Url.Replace('-', '+').Replace('_', '/');

        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }

        var bytes = Convert.FromBase64String(base64);
        return Encoding.UTF8.GetString(bytes);
    }

    internal static string CreateRawMessage(string to, string subject, string body)
    {
        var message = new StringBuilder();
        message.AppendLine($"To: {to}");
        message.AppendLine($"Subject: {subject}");
        message.AppendLine($"Content-Type: {MediaTypeNames.Text.Plain}; charset=utf-8");
        message.AppendLine();
        message.Append(body);

        var bytes = Encoding.UTF8.GetBytes(message.ToString());
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}
