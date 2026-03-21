namespace Itdg.Crm.Api.Infrastructure.Services;

using System.Security.Cryptography;
using Itdg.Crm.Api.Infrastructure.Options;
using Microsoft.Extensions.Options;

public class AesTokenEncryptionService : ITokenEncryptionService
{
    private readonly byte[] _key;

    public AesTokenEncryptionService(IOptions<TokenEncryptionOptions> options)
    {
        _key = Convert.FromBase64String(options.Value.EncryptionKey);
    }

    public string Encrypt(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        // Prepend IV to ciphertext for storage
        var result = new byte[aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string cipherText)
    {
        var fullCipher = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = _key;

        var ivLength = aes.BlockSize / 8;
        var iv = new byte[ivLength];
        var cipherBytes = new byte[fullCipher.Length - ivLength];

        Buffer.BlockCopy(fullCipher, 0, iv, 0, ivLength);
        Buffer.BlockCopy(fullCipher, ivLength, cipherBytes, 0, cipherBytes.Length);

        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

        return System.Text.Encoding.UTF8.GetString(plainBytes);
    }
}
