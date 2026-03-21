namespace Itdg.Crm.Api.Application.QueryHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetClientEmailsHandler : IQueryHandler<GetClientEmails, PaginatedResultDto<EmailMirrorDto>>
{
    private readonly IEmailMirrorRepository _emailMirrorRepository;
    private readonly IUserRepository _userRepository;
    private readonly IClientAssignmentRepository _clientAssignmentRepository;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ILogger<GetClientEmailsHandler> _logger;

    public GetClientEmailsHandler(
        IEmailMirrorRepository emailMirrorRepository,
        IUserRepository userRepository,
        IClientAssignmentRepository clientAssignmentRepository,
        ICurrentUserProvider currentUserProvider,
        ILogger<GetClientEmailsHandler> logger)
    {
        _emailMirrorRepository = emailMirrorRepository;
        _userRepository = userRepository;
        _clientAssignmentRepository = clientAssignmentRepository;
        _currentUserProvider = currentUserProvider;
        _logger = logger;
    }

    public async Task<PaginatedResultDto<EmailMirrorDto>> HandleAsync(
        GetClientEmails query,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Get Client Emails");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation(
            "Getting emails for client {ClientId} page {Page} | CorrelationId: {CorrelationId}",
            query.ClientId, query.Page, correlationId);

        if (!_currentUserProvider.IsInRole(nameof(UserRole.Administrator)))
        {
            var entraObjectId = _currentUserProvider.GetEntraObjectId();
            if (string.IsNullOrWhiteSpace(entraObjectId))
            {
                throw new ForbiddenException();
            }

            var user = await _userRepository.GetByEntraObjectIdAsync(entraObjectId, cancellationToken);
            if (user is null)
            {
                throw new ForbiddenException();
            }

            var isAssigned = await _clientAssignmentRepository.ExistsAsync(user.Id, query.ClientId, cancellationToken);
            if (!isAssigned)
            {
                throw new ForbiddenException();
            }
        }

        var (items, totalCount) = await _emailMirrorRepository.GetPagedByClientIdAsync(
            query.ClientId,
            query.Page,
            query.PageSize,
            query.Search,
            cancellationToken);

        var emailDtos = items.Select(email => new EmailMirrorDto(
            EmailId: email.Id,
            ClientId: email.ClientId,
            GmailMessageId: email.GmailMessageId,
            GmailThreadId: email.GmailThreadId,
            Subject: email.Subject,
            From: email.From,
            To: email.To,
            BodyPreview: email.BodyPreview,
            HasAttachments: email.HasAttachments,
            ReceivedAt: email.ReceivedAt
        )).ToList();

        return new PaginatedResultDto<EmailMirrorDto>(
            Items: emailDtos,
            TotalCount: totalCount,
            Page: query.Page,
            PageSize: query.PageSize
        );
    }
}
