namespace Itdg.Crm.Api.Test.Services;

using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Infrastructure.Options;
using Itdg.Crm.Api.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class AzureOpenAiDraftingServiceTests
{
    private readonly IOptions<AzureOpenAiOptions> _options;
    private readonly ILogger<AzureOpenAiDraftingService> _logger;

    public AzureOpenAiDraftingServiceTests()
    {
        var azureOpenAiOptions = new AzureOpenAiOptions
        {
            Endpoint = "https://test.openai.azure.com/",
            ApiKey = "test-api-key",
            DeploymentName = "gpt-4o"
        };
        _options = Options.Create(azureOpenAiOptions);
        _logger = Substitute.For<ILogger<AzureOpenAiDraftingService>>();
    }

    [Fact]
    public void Constructor_WithValidOptions_CreatesInstance()
    {
        // Act
        var service = new AzureOpenAiDraftingService(_options, _logger);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void GetSystemPrompt_WithEnglishLanguage_ReturnsEnglishPrompt()
    {
        // Act
        string prompt = AzureOpenAiDraftingService.GetSystemPrompt("en");

        // Assert
        prompt.Should().Contain("professional email drafting assistant");
        prompt.Should().Contain("tax consulting practice in Puerto Rico");
        prompt.Should().Contain("ITDG Tax Consulting Group");
    }

    [Fact]
    public void GetSystemPrompt_WithSpanishLanguage_ReturnsSpanishPrompt()
    {
        // Act
        string prompt = AzureOpenAiDraftingService.GetSystemPrompt("es");

        // Assert
        prompt.Should().Contain("asistente profesional de redacción");
        prompt.Should().Contain("consultoría contributiva en Puerto Rico");
        prompt.Should().Contain("ITDG Tax Consulting Group");
    }

    [Fact]
    public void GetSystemPrompt_WithSpanishPrLocale_ReturnsSpanishPrompt()
    {
        // Act
        string prompt = AzureOpenAiDraftingService.GetSystemPrompt("es-pr");

        // Assert
        prompt.Should().Contain("asistente profesional de redacción");
    }

    [Fact]
    public void GetSystemPrompt_WithEnglishPrLocale_ReturnsEnglishPrompt()
    {
        // Act
        string prompt = AzureOpenAiDraftingService.GetSystemPrompt("en-pr");

        // Assert
        prompt.Should().Contain("professional email drafting assistant");
    }

    [Theory]
    [InlineData("EN")]
    [InlineData("En")]
    [InlineData("en-US")]
    [InlineData("en-pr")]
    public void GetSystemPrompt_WithEnglishVariants_ReturnsEnglishPrompt(string language)
    {
        // Act
        string prompt = AzureOpenAiDraftingService.GetSystemPrompt(language);

        // Assert
        prompt.Should().Contain("professional email drafting assistant");
    }

    [Theory]
    [InlineData("ES")]
    [InlineData("Es")]
    [InlineData("es-PR")]
    [InlineData("es-pr")]
    public void GetSystemPrompt_WithSpanishVariants_ReturnsSpanishPrompt(string language)
    {
        // Act
        string prompt = AzureOpenAiDraftingService.GetSystemPrompt(language);

        // Assert
        prompt.Should().Contain("asistente profesional de redacción");
    }

    [Fact]
    public void GetSystemPrompt_WithUnknownLanguage_DefaultsToEnglish()
    {
        // Act
        string prompt = AzureOpenAiDraftingService.GetSystemPrompt("fr");

        // Assert
        prompt.Should().Contain("professional email drafting assistant");
    }

    [Fact]
    public void BuildUserPrompt_WithEnglishLanguage_ContainsEnglishInstruction()
    {
        // Arrange
        var request = new AiDraftRequest("John Doe", "Tax return filing deadline", "en");

        // Act
        string prompt = AzureOpenAiDraftingService.BuildUserPrompt(request);

        // Assert
        prompt.Should().Contain("John Doe");
        prompt.Should().Contain("Tax return filing deadline");
        prompt.Should().Contain("Write the email in English.");
    }

    [Fact]
    public void BuildUserPrompt_WithSpanishLanguage_ContainsSpanishInstruction()
    {
        // Arrange
        var request = new AiDraftRequest("María García", "Fecha límite de radicación", "es");

        // Act
        string prompt = AzureOpenAiDraftingService.BuildUserPrompt(request);

        // Assert
        prompt.Should().Contain("María García");
        prompt.Should().Contain("Fecha límite de radicación");
        prompt.Should().Contain("Redacta el correo electrónico en español.");
    }

    [Fact]
    public void BuildUserPrompt_WithAdditionalContext_IncludesContext()
    {
        // Arrange
        var request = new AiDraftRequest("John Doe", "Tax filing", "en", "Client has outstanding balance of $5,000");

        // Act
        string prompt = AzureOpenAiDraftingService.BuildUserPrompt(request);

        // Assert
        prompt.Should().Contain("Additional context: Client has outstanding balance of $5,000");
    }

    [Fact]
    public void BuildUserPrompt_WithoutAdditionalContext_DoesNotIncludeContextSection()
    {
        // Arrange
        var request = new AiDraftRequest("John Doe", "Tax filing", "en");

        // Act
        string prompt = AzureOpenAiDraftingService.BuildUserPrompt(request);

        // Assert
        prompt.Should().NotContain("Additional context:");
    }

    [Fact]
    public void BuildUserPrompt_WithEmptyAdditionalContext_DoesNotIncludeContextSection()
    {
        // Arrange
        var request = new AiDraftRequest("John Doe", "Tax filing", "en", "   ");

        // Act
        string prompt = AzureOpenAiDraftingService.BuildUserPrompt(request);

        // Assert
        prompt.Should().NotContain("Additional context:");
    }

    [Fact]
    public void BuildUserPrompt_ContainsDraftInstruction()
    {
        // Arrange
        var request = new AiDraftRequest("John Doe", "Tax filing", "en");

        // Act
        string prompt = AzureOpenAiDraftingService.BuildUserPrompt(request);

        // Assert
        prompt.Should().Contain("Draft a professional email");
        prompt.Should().Contain("Client: John Doe");
        prompt.Should().Contain("Topic: Tax filing");
    }

    [Fact]
    public void GetSystemPrompt_EnglishPrompt_ContainsTaxConsultingGuidelines()
    {
        // Act
        string prompt = AzureOpenAiDraftingService.GetSystemPrompt("en");

        // Assert
        prompt.Should().Contain("Puerto Rico tax regulations");
        prompt.Should().Contain("IRS requirements");
        prompt.Should().Contain("Do not fabricate specific tax figures");
    }

    [Fact]
    public void GetSystemPrompt_SpanishPrompt_ContainsTaxConsultingGuidelines()
    {
        // Act
        string prompt = AzureOpenAiDraftingService.GetSystemPrompt("es");

        // Assert
        prompt.Should().Contain("regulaciones contributivas de Puerto Rico");
        prompt.Should().Contain("requisitos del IRS");
        prompt.Should().Contain("No inventes cifras contributivas específicas");
    }
}
