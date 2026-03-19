namespace Itdg.Crm.Api.Application.Exceptions;

public class ConflictException : DomainException
{
    public ConflictException(string message, string errorCode = "conflict")
        : base(message, errorCode)
    {
    }
}
