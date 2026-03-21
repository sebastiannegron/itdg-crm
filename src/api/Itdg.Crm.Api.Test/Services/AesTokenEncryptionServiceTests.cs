namespace Itdg.Crm.Api.Test.Services;

using System.Security.Cryptography;
using Itdg.Crm.Api.Infrastructure.Options;
using Itdg.Crm.Api.Infrastructure.Services;
using Microsoft.Extensions.Options;

public class AesTokenEncryptionServiceTests
{
    private readonly AesTokenEncryptionService _service;

    public AesTokenEncryptionServiceTests()
    {
        // Generate a valid 256-bit AES key
        using var aes = Aes.Create();
        aes.GenerateKey();
        var base64Key = Convert.ToBase64String(aes.Key);

        var options = Options.Create(new TokenEncryptionOptions { EncryptionKey = base64Key });
        _service = new AesTokenEncryptionService(options);
    }

    [Fact]
    public void Encrypt_ReturnsBase64String()
    {
        // Act
        var encrypted = _service.Encrypt("test-token");

        // Assert
        encrypted.Should().NotBeNullOrEmpty();
        var bytes = Convert.FromBase64String(encrypted);
        bytes.Should().NotBeEmpty();
    }

    [Fact]
    public void Decrypt_ReturnsOriginalText()
    {
        // Arrange
        var original = "ya29.a0access-token-value";
        var encrypted = _service.Encrypt(original);

        // Act
        var decrypted = _service.Decrypt(encrypted);

        // Assert
        decrypted.Should().Be(original);
    }

    [Fact]
    public void Encrypt_ProducesDifferentCiphertexts_ForSamePlaintext()
    {
        // Arrange
        var plainText = "same-token-value";

        // Act
        var encrypted1 = _service.Encrypt(plainText);
        var encrypted2 = _service.Encrypt(plainText);

        // Assert (different IVs produce different ciphertexts)
        encrypted1.Should().NotBe(encrypted2);
    }

    [Fact]
    public void Decrypt_SucceedsForBothDifferentCiphertexts()
    {
        // Arrange
        var plainText = "same-token-value";
        var encrypted1 = _service.Encrypt(plainText);
        var encrypted2 = _service.Encrypt(plainText);

        // Act
        var decrypted1 = _service.Decrypt(encrypted1);
        var decrypted2 = _service.Decrypt(encrypted2);

        // Assert
        decrypted1.Should().Be(plainText);
        decrypted2.Should().Be(plainText);
    }

    [Fact]
    public void RoundTrip_WorksForLongTokens()
    {
        // Arrange
        var longToken = new string('A', 2048);
        var encrypted = _service.Encrypt(longToken);

        // Act
        var decrypted = _service.Decrypt(encrypted);

        // Assert
        decrypted.Should().Be(longToken);
    }

    [Fact]
    public void RoundTrip_WorksForSpecialCharacters()
    {
        // Arrange
        var tokenWithSpecialChars = "ya29.a0AfB_byC-token/with+special=chars&more?query#fragment";
        var encrypted = _service.Encrypt(tokenWithSpecialChars);

        // Act
        var decrypted = _service.Decrypt(encrypted);

        // Assert
        decrypted.Should().Be(tokenWithSpecialChars);
    }
}
