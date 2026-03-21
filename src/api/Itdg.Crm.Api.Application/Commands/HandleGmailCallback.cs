namespace Itdg.Crm.Api.Application.Commands;

using Itdg.Crm.Api.Application.Abstractions;

public record HandleGmailCallback(string Code) : ICommand;
