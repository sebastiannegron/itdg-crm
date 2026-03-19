namespace Itdg.Crm.Api.Application.Exceptions;

public class DomainException : Exception
{
    public string ErrorCode { get; }

    public DomainException(string message, string errorCode)
        : base(message)
    {
        ErrorCode = errorCode;
    }

    public DomainException(string message, string errorCode, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}
