namespace Itdg.Crm.Api.Application.Exceptions;

public class NotFoundException : DomainException
{
    public NotFoundException(string entityName, object key)
        : base($"{entityName} with key '{key}' was not found.", $"{entityName.ToLowerInvariant()}_not_found")
    {
    }
}
