namespace Itdg.Crm.Api.Application.Abstractions;

public interface ICommandHandler<in TCommand> where TCommand : class, ICommand
{
    Task HandleAsync(TCommand command, string language, Guid correlationId, CancellationToken cancellationToken);
}
