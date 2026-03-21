namespace Itdg.Crm.Api.Application.Abstractions;

using Itdg.Crm.Api.Application.Dtos;

public interface IAiDraftingService
{
    Task<string> GenerateDraftAsync(AiDraftRequest request, CancellationToken cancellationToken);
}
