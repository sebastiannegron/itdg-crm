namespace Itdg.Crm.Api.Application.Abstractions;

public interface ITokenEncryptionService
{
    string Encrypt(string plainText);
    string Decrypt(string cipherText);
}
