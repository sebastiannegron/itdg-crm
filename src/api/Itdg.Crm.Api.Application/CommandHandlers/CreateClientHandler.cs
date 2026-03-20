namespace Itdg.Crm.Api.Application.CommandHandlers;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class CreateClientHandler : ICommandHandler<CreateClient>
{
    private readonly IClientRepository _repository;
    private readonly ITenantProvider _tenantProvider;
    private readonly IGoogleDriveService _driveService;
    private readonly IGoogleDriveTokenProvider _tokenProvider;
    private readonly IGenericRepository<DocumentCategory> _categoryRepository;
    private readonly ILogger<CreateClientHandler> _logger;

    public CreateClientHandler(
        IClientRepository repository,
        ITenantProvider tenantProvider,
        IGoogleDriveService driveService,
        IGoogleDriveTokenProvider tokenProvider,
        IGenericRepository<DocumentCategory> categoryRepository,
        ILogger<CreateClientHandler> logger)
    {
        _repository = repository;
        _tenantProvider = tenantProvider;
        _driveService = driveService;
        _tokenProvider = tokenProvider;
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task HandleAsync(CreateClient command, string language, Guid correlationId, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Create Client");
        activity?.SetTag("CorrelationId", correlationId);

        _logger.LogInformation("Creating client {ClientName} | CorrelationId: {CorrelationId}", command.Name, correlationId);

        var client = new Client
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            ContactEmail = command.ContactEmail,
            Phone = command.Phone,
            Address = command.Address,
            TierId = command.TierId,
            Status = command.Status,
            IndustryTag = command.IndustryTag,
            Notes = command.Notes,
            CustomFields = command.CustomFields,
            TenantId = _tenantProvider.GetTenantId()
        };

        await _repository.AddAsync(client, cancellationToken);

        _logger.LogInformation("Client {ClientId} created successfully | CorrelationId: {CorrelationId}", client.Id, correlationId);

        await CreateDriveFolderStructureAsync(client.Name, correlationId, cancellationToken);
    }

    private async Task CreateDriveFolderStructureAsync(string clientName, Guid correlationId, CancellationToken cancellationToken)
    {
        var accessToken = _tokenProvider.GetAccessToken();
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            _logger.LogWarning("Google Drive access token not available, skipping folder structure creation for client {ClientName} | CorrelationId: {CorrelationId}",
                clientName, correlationId);
            return;
        }

        try
        {
            using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Create Client Drive Folders");
            activity?.SetTag("CorrelationId", correlationId);
            activity?.SetTag("ClientName", clientName);

            var clientFolder = await _driveService.CreateFolderAsync(accessToken, clientName, cancellationToken: cancellationToken);

            var year = DateTimeOffset.UtcNow.Year.ToString();
            var yearFolder = await _driveService.CreateFolderAsync(accessToken, year, clientFolder.Id, cancellationToken);

            var categories = await _categoryRepository.GetAllAsync(cancellationToken);
            foreach (var category in categories.OrderBy(c => c.SortOrder))
            {
                var folderName = !string.IsNullOrWhiteSpace(category.NamingConvention)
                    ? category.NamingConvention
                    : category.Name;

                await _driveService.CreateFolderAsync(accessToken, folderName, yearFolder.Id, cancellationToken);
            }

            _logger.LogInformation("Google Drive folder structure created for client {ClientName} | CorrelationId: {CorrelationId}",
                clientName, correlationId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create Google Drive folder structure for client {ClientName} | CorrelationId: {CorrelationId}",
                clientName, correlationId);
        }
    }
}
