namespace Itdg.Crm.Api.Application.Commands;

using Itdg.Crm.Api.Application.Abstractions;

public record HandleGoogleCallback(string Code) : ICommand;
