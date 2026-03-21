namespace Itdg.Crm.Api.Infrastructure.Services;

using System.ClientModel;
using System.Diagnostics;
using Azure;
using Azure.AI.OpenAI;
using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

public class AzureOpenAiDraftingService : IAiDraftingService
{
    private readonly AzureOpenAiOptions _options;
    private readonly ILogger<AzureOpenAiDraftingService> _logger;

    private const string SystemPromptEnglish =
        """
        You are a professional email drafting assistant for a tax consulting practice in Puerto Rico (ITDG Tax Consulting Group).
        Your role is to help associates draft clear, professional, and courteous client emails.

        Guidelines:
        - Write in a professional yet approachable tone appropriate for client communications.
        - Be mindful of Puerto Rico tax regulations, IRS requirements, and compliance considerations.
        - Include appropriate greetings and closings for business correspondence.
        - Keep the language clear and concise, avoiding unnecessary jargon.
        - When referencing tax concepts, use accurate terminology.
        - Do not fabricate specific tax figures, deadlines, or legal advice — use placeholders where specifics are needed.
        - Format the email with proper structure: greeting, body, closing, and signature placeholder.

        Respond only with the email draft text. Do not include explanations or meta-commentary outside the email content.
        """;

    private const string SystemPromptSpanish =
        """
        Eres un asistente profesional de redacción de correos electrónicos para una práctica de consultoría contributiva en Puerto Rico (ITDG Tax Consulting Group).
        Tu rol es ayudar a los asociados a redactar correos electrónicos claros, profesionales y corteses para los clientes.

        Directrices:
        - Escribe en un tono profesional pero accesible, apropiado para comunicaciones con clientes.
        - Ten en cuenta las regulaciones contributivas de Puerto Rico, los requisitos del IRS y las consideraciones de cumplimiento.
        - Incluye saludos y despedidas apropiados para correspondencia empresarial.
        - Mantén el lenguaje claro y conciso, evitando jerga innecesaria.
        - Al referirte a conceptos contributivos, utiliza terminología precisa.
        - No inventes cifras contributivas específicas, fechas límite o asesoría legal — usa marcadores de posición donde se necesiten datos específicos.
        - Estructura el correo electrónico correctamente: saludo, cuerpo, despedida y marcador de posición para la firma.

        Responde únicamente con el texto del borrador del correo electrónico. No incluyas explicaciones ni metacomentarios fuera del contenido del correo.
        """;

    public AzureOpenAiDraftingService(IOptions<AzureOpenAiOptions> options, ILogger<AzureOpenAiDraftingService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> GenerateDraftAsync(AiDraftRequest request, CancellationToken cancellationToken)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Generate AI Draft");
        activity?.SetTag("Language", request.Language);

        _logger.LogInformation("Generating AI email draft for client {ClientName} in {Language}", request.ClientName, request.Language);

        string systemPrompt = GetSystemPrompt(request.Language);
        string userPrompt = BuildUserPrompt(request);

        AzureOpenAIClient client = new(new Uri(_options.Endpoint), new AzureKeyCredential(_options.ApiKey));
        ChatClient chatClient = client.GetChatClient(_options.DeploymentName);

        List<ChatMessage> messages =
        [
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt)
        ];

        ChatCompletionOptions completionOptions = new()
        {
            Temperature = 0.7f,
            MaxOutputTokenCount = 2048
        };

        ClientResult<ChatCompletion> response = await chatClient.CompleteChatAsync(messages, completionOptions, cancellationToken);

        string draft = response.Value.Content[0].Text;

        _logger.LogInformation("Successfully generated AI email draft for client {ClientName}", request.ClientName);

        return draft;
    }

    internal static string GetSystemPrompt(string language)
    {
        return language.StartsWith("es", StringComparison.OrdinalIgnoreCase)
            ? SystemPromptSpanish
            : SystemPromptEnglish;
    }

    internal static string BuildUserPrompt(AiDraftRequest request)
    {
        string languageInstruction = request.Language.StartsWith("es", StringComparison.OrdinalIgnoreCase)
            ? "Redacta el correo electrónico en español."
            : "Write the email in English.";

        string prompt = $"""
            Draft a professional email for the following scenario:

            Client: {request.ClientName}
            Topic: {request.Topic}
            {languageInstruction}
            """;

        if (!string.IsNullOrWhiteSpace(request.AdditionalContext))
        {
            prompt += $"\nAdditional context: {request.AdditionalContext}";
        }

        return prompt;
    }
}
