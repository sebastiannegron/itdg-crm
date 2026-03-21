namespace Itdg.Crm.Api.Test.Services;

using Itdg.Crm.Api.Infrastructure.Options;
using Itdg.Crm.Api.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Abstractions.Serialization;

public class MicrosoftGraphEmailSenderTests
{
    private readonly MicrosoftGraphEmailOptions _emailOptions;
    private readonly IOptions<MicrosoftGraphEmailOptions> _options;
    private readonly ILogger<MicrosoftGraphEmailSender> _logger;

    public MicrosoftGraphEmailSenderTests()
    {
        _emailOptions = new MicrosoftGraphEmailOptions
        {
            TenantId = "test-tenant-id",
            ClientId = "test-client-id",
            ClientSecret = "test-client-secret",
            SenderAddress = "noreply@example.com"
        };
        _options = Options.Create(_emailOptions);
        _logger = Substitute.For<ILogger<MicrosoftGraphEmailSender>>();
    }

    [Fact]
    public void BuildMessage_WithTextContent_SetsBodyTypeToText()
    {
        // Act
        var message = MicrosoftGraphEmailSender.BuildMessage(
            "recipient@example.com", "Test Subject", "Plain text body", BodyType.Text);

        // Assert
        message.Body.Should().NotBeNull();
        message.Body!.ContentType.Should().Be(BodyType.Text);
        message.Body.Content.Should().Be("Plain text body");
    }

    [Fact]
    public void BuildMessage_WithHtmlContent_SetsBodyTypeToHtml()
    {
        // Act
        var message = MicrosoftGraphEmailSender.BuildMessage(
            "recipient@example.com", "Test Subject", "<h1>Hello</h1>", BodyType.Html);

        // Assert
        message.Body.Should().NotBeNull();
        message.Body!.ContentType.Should().Be(BodyType.Html);
        message.Body.Content.Should().Be("<h1>Hello</h1>");
    }

    [Fact]
    public void BuildMessage_SetsCorrectSubject()
    {
        // Act
        var message = MicrosoftGraphEmailSender.BuildMessage(
            "recipient@example.com", "Important Notification", "Body content", BodyType.Text);

        // Assert
        message.Subject.Should().Be("Important Notification");
    }

    [Fact]
    public void BuildMessage_SetsCorrectRecipient()
    {
        // Act
        var message = MicrosoftGraphEmailSender.BuildMessage(
            "recipient@example.com", "Subject", "Body", BodyType.Text);

        // Assert
        message.ToRecipients.Should().HaveCount(1);
        message.ToRecipients![0].EmailAddress!.Address.Should().Be("recipient@example.com");
    }

    [Fact]
    public void BuildMessage_WithEmptyBody_CreatesMessageWithEmptyContent()
    {
        // Act
        var message = MicrosoftGraphEmailSender.BuildMessage(
            "recipient@example.com", "Subject", "", BodyType.Text);

        // Assert
        message.Body!.Content.Should().BeEmpty();
    }

    [Fact]
    public void BuildMessage_WithHtmlTemplate_PreservesHtmlContent()
    {
        // Arrange
        const string htmlContent = """
            <html>
            <body>
                <h1>Welcome</h1>
                <p>Your account has been created.</p>
            </body>
            </html>
            """;

        // Act
        var message = MicrosoftGraphEmailSender.BuildMessage(
            "user@example.com", "Welcome", htmlContent, BodyType.Html);

        // Assert
        message.Body!.Content.Should().Contain("<h1>Welcome</h1>");
        message.Body.Content.Should().Contain("<p>Your account has been created.</p>");
        message.Body.ContentType.Should().Be(BodyType.Html);
    }

    [Fact]
    public async Task SendAsync_CallsGraphClient_WithTextBodyType()
    {
        // Arrange
        var requestAdapter = Substitute.For<IRequestAdapter>();
        requestAdapter.BaseUrl = "https://graph.microsoft.com/v1.0";
        var graphClient = new GraphServiceClient(requestAdapter);
        var sender = new MicrosoftGraphEmailSender(_emailOptions, _logger, graphClient);

        RequestInformation? capturedRequest = null;
        requestAdapter.SendNoContentAsync(
            Arg.Do<RequestInformation>(ri => capturedRequest = ri),
            Arg.Any<Dictionary<string, ParsableFactory<IParsable>>>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await sender.SendAsync("recipient@example.com", "Test Subject", "Test Body");

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.PathParameters.Should().ContainKey("user%2Did");
        capturedRequest.PathParameters["user%2Did"].Should().Be("noreply@example.com");
    }

    [Fact]
    public async Task SendHtmlAsync_CallsGraphClient_WithHtmlBodyType()
    {
        // Arrange
        var requestAdapter = Substitute.For<IRequestAdapter>();
        requestAdapter.BaseUrl = "https://graph.microsoft.com/v1.0";
        var graphClient = new GraphServiceClient(requestAdapter);
        var sender = new MicrosoftGraphEmailSender(_emailOptions, _logger, graphClient);

        RequestInformation? capturedRequest = null;
        requestAdapter.SendNoContentAsync(
            Arg.Do<RequestInformation>(ri => capturedRequest = ri),
            Arg.Any<Dictionary<string, ParsableFactory<IParsable>>>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await sender.SendHtmlAsync("recipient@example.com", "HTML Subject", "<p>HTML Body</p>");

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.PathParameters.Should().ContainKey("user%2Did");
        capturedRequest.PathParameters["user%2Did"].Should().Be("noreply@example.com");
    }

    [Fact]
    public async Task SendAsync_WhenGraphClientThrows_PropagatesException()
    {
        // Arrange
        var requestAdapter = Substitute.For<IRequestAdapter>();
        requestAdapter.BaseUrl = "https://graph.microsoft.com/v1.0";
        requestAdapter.SendNoContentAsync(
            Arg.Any<RequestInformation>(),
            Arg.Any<Dictionary<string, ParsableFactory<IParsable>>>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new ServiceException("Graph API error")));

        var graphClient = new GraphServiceClient(requestAdapter);
        var sender = new MicrosoftGraphEmailSender(_emailOptions, _logger, graphClient);

        // Act & Assert
        var act = async () => await sender.SendAsync("recipient@example.com", "Subject", "Body");
        await act.Should().ThrowAsync<ServiceException>().WithMessage("*Graph API error*");
    }

    [Fact]
    public async Task SendHtmlAsync_WhenGraphClientThrows_PropagatesException()
    {
        // Arrange
        var requestAdapter = Substitute.For<IRequestAdapter>();
        requestAdapter.BaseUrl = "https://graph.microsoft.com/v1.0";
        requestAdapter.SendNoContentAsync(
            Arg.Any<RequestInformation>(),
            Arg.Any<Dictionary<string, ParsableFactory<IParsable>>>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new ServiceException("Graph API error")));

        var graphClient = new GraphServiceClient(requestAdapter);
        var sender = new MicrosoftGraphEmailSender(_emailOptions, _logger, graphClient);

        // Act & Assert
        var act = async () => await sender.SendHtmlAsync("recipient@example.com", "Subject", "<p>Body</p>");
        await act.Should().ThrowAsync<ServiceException>().WithMessage("*Graph API error*");
    }

    [Fact]
    public async Task SendAsync_UsesSenderAddressFromOptions()
    {
        // Arrange
        var customOptions = new MicrosoftGraphEmailOptions
        {
            TenantId = "test-tenant",
            ClientId = "test-client",
            ClientSecret = "test-secret",
            SenderAddress = "custom-sender@domain.com"
        };

        var requestAdapter = Substitute.For<IRequestAdapter>();
        requestAdapter.BaseUrl = "https://graph.microsoft.com/v1.0";
        var graphClient = new GraphServiceClient(requestAdapter);
        var sender = new MicrosoftGraphEmailSender(customOptions, _logger, graphClient);

        RequestInformation? capturedRequest = null;
        requestAdapter.SendNoContentAsync(
            Arg.Do<RequestInformation>(ri => capturedRequest = ri),
            Arg.Any<Dictionary<string, ParsableFactory<IParsable>>>(),
            Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await sender.SendAsync("recipient@example.com", "Test", "Body");

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.PathParameters["user%2Did"].Should().Be("custom-sender@domain.com");
    }
}
