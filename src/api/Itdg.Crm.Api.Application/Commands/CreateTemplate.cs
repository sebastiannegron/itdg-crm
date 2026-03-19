namespace Itdg.Crm.Api.Application.Commands;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Domain.Enums;

public record CreateTemplate(
    TemplateCategory Category,
    string Name,
    string SubjectTemplate,
    string BodyTemplate,
    string Language,
    Guid CreatedById
) : ICommand;
