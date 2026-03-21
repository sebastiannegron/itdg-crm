namespace Itdg.Crm.Api.Application.QueryHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetClientTimelineHandler : IQueryHandler<GetClientTimeline, PaginatedResultDto<TimelineItemDto>>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IEmailMirrorRepository _emailMirrorRepository;
    private readonly IUserRepository _userRepository;
    private readonly IClientAssignmentRepository _clientAssignmentRepository;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ILogger<GetClientTimelineHandler> _logger;

    public GetClientTimelineHandler(
        IDocumentRepository documentRepository,
        IMessageRepository messageRepository,
        IEmailMirrorRepository emailMirrorRepository,
        IUserRepository userRepository,
        IClientAssignmentRepository clientAssignmentRepository,
        ICurrentUserProvider currentUserProvider,
        ILogger<GetClientTimelineHandler> logger)
    {
        _documentRepository = documentRepository;
        _messageRepository = messageRepository;
        _emailMirrorRepository = emailMirrorRepository;
        _userRepository = userRepository;
        _clientAssignmentRepository = clientAssignmentRepository;
        _currentUserProvider = currentUserProvider;
        _logger = logger;
    }

    public async Task<PaginatedResultDto<TimelineItemDto>> HandleAsync(
        GetClientTimeline query,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Get Client Timeline");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation(
            "Getting timeline for client {ClientId} page {Page} | CorrelationId: {CorrelationId}",
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

        var documents = await _documentRepository.GetByClientIdAsync(query.ClientId, cancellationToken);
        var messages = await _messageRepository.GetByClientIdAsync(query.ClientId, cancellationToken);
        var emails = await _emailMirrorRepository.GetByClientIdAsync(query.ClientId, cancellationToken);

        var timelineItems = new List<TimelineItemDto>();

        foreach (var doc in documents)
        {
            timelineItems.Add(new TimelineItemDto(
                Id: doc.Id,
                Type: "document",
                Description: doc.FileName,
                Timestamp: doc.CreatedAt,
                Actor: null
            ));
        }

        foreach (var msg in messages)
        {
            timelineItems.Add(new TimelineItemDto(
                Id: msg.Id,
                Type: "message",
                Description: msg.Subject,
                Timestamp: msg.CreatedAt,
                Actor: null
            ));
        }

        foreach (var email in emails)
        {
            timelineItems.Add(new TimelineItemDto(
                Id: email.Id,
                Type: "email",
                Description: email.Subject,
                Timestamp: email.ReceivedAt,
                Actor: email.From
            ));
        }

        var sorted = timelineItems
            .OrderByDescending(t => t.Timestamp)
            .ToList();

        var totalCount = sorted.Count;
        var paged = sorted
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        return new PaginatedResultDto<TimelineItemDto>(
            Items: paged,
            TotalCount: totalCount,
            Page: query.Page,
            PageSize: query.PageSize
        );
    }
}
