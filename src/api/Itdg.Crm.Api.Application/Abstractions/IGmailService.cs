namespace Itdg.Crm.Api.Application.Abstractions;

using Itdg.Crm.Api.Application.Dtos;

/// <summary>
/// Provides Gmail API operations using per-user OAuth 2.0 tokens.
/// </summary>
public interface IGmailService
{
    /// <summary>
    /// Lists Gmail messages for the authenticated user, optionally filtered by query.
    /// </summary>
    /// <param name="userAccessToken">The user's OAuth 2.0 access token.</param>
    /// <param name="query">Optional Gmail search query (e.g., "from:client@example.com").</param>
    /// <param name="maxResults">Maximum number of messages to return (default 20).</param>
    /// <param name="pageToken">Token for pagination of results.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated list of Gmail messages.</returns>
    Task<GmailMessageListDto> ListMessagesAsync(
        string userAccessToken,
        string? query = null,
        int maxResults = 20,
        string? pageToken = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a single Gmail message by its ID.
    /// </summary>
    /// <param name="userAccessToken">The user's OAuth 2.0 access token.</param>
    /// <param name="messageId">The Gmail message ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The Gmail message details.</returns>
    Task<GmailMessageDto> GetMessageAsync(
        string userAccessToken,
        string messageId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an email via Gmail on behalf of the authenticated user.
    /// </summary>
    /// <param name="userAccessToken">The user's OAuth 2.0 access token.</param>
    /// <param name="to">Recipient email address.</param>
    /// <param name="subject">Email subject.</param>
    /// <param name="body">Email body (plain text).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The sent message details.</returns>
    Task<GmailMessageDto> SendMessageAsync(
        string userAccessToken,
        string to,
        string subject,
        string body,
        CancellationToken cancellationToken = default);
}
