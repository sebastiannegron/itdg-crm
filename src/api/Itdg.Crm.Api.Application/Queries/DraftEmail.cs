namespace Itdg.Crm.Api.Application.Queries;

using Itdg.Crm.Api.Application.Abstractions;

public record DraftEmail(string ClientName, string Topic, string Language, string? AdditionalContext = null) : IQuery<string>;
