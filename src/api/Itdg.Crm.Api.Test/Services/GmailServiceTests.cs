namespace Itdg.Crm.Api.Test.Services;

using Google.Apis.Gmail.v1.Data;
using Itdg.Crm.Api.Infrastructure.Options;
using Itdg.Crm.Api.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class GmailServiceTests
{
    private readonly IOptions<GmailOptions> _options;
    private readonly ILogger<GmailService> _logger;

    public GmailServiceTests()
    {
        var gmailOptions = new GmailOptions
        {
            ClientId = "test-client-id",
            ClientSecret = "test-client-secret",
            ApplicationName = "Test-App"
        };

        _options = Microsoft.Extensions.Options.Options.Create(gmailOptions);
        _logger = Substitute.For<ILogger<GmailService>>();
    }

    [Fact]
    public void MapToDto_MapsBasicFieldsCorrectly()
    {
        // Arrange
        var message = new Message
        {
            Id = "msg-123",
            ThreadId = "thread-456",
            Snippet = "Hello world snippet",
            LabelIds = new List<string> { "INBOX", "UNREAD" },
            Payload = new MessagePart
            {
                Headers = new List<MessagePartHeader>
                {
                    new() { Name = "Subject", Value = "Test Subject" },
                    new() { Name = "From", Value = "sender@example.com" },
                    new() { Name = "To", Value = "recipient@example.com" },
                    new() { Name = "Date", Value = "Mon, 20 Jan 2025 10:30:00 -0400" }
                },
                Body = new MessagePartBody { Data = EncodeBase64Url("Hello body") }
            }
        };

        // Act
        var dto = GmailService.MapToDto(message);

        // Assert
        dto.Id.Should().Be("msg-123");
        dto.ThreadId.Should().Be("thread-456");
        dto.Subject.Should().Be("Test Subject");
        dto.From.Should().Be("sender@example.com");
        dto.To.Should().Be("recipient@example.com");
        dto.Snippet.Should().Be("Hello world snippet");
        dto.LabelIds.Should().BeEquivalentTo(new[] { "INBOX", "UNREAD" });
        dto.Date.Should().Be(new DateTimeOffset(2025, 1, 20, 10, 30, 0, TimeSpan.FromHours(-4)));
    }

    [Fact]
    public void MapToDto_HandlesNullPayload()
    {
        // Arrange
        var message = new Message
        {
            Id = "msg-empty",
            ThreadId = "thread-empty",
            Snippet = "Fallback snippet",
            LabelIds = null,
            Payload = null
        };

        // Act
        var dto = GmailService.MapToDto(message);

        // Assert
        dto.Id.Should().Be("msg-empty");
        dto.Subject.Should().BeEmpty();
        dto.From.Should().BeEmpty();
        dto.To.Should().BeEmpty();
        dto.BodyPreview.Should().BeEmpty();
        dto.HasAttachments.Should().BeFalse();
        dto.LabelIds.Should().BeEmpty();
    }

    [Fact]
    public void MapToDto_HandlesNullHeaders()
    {
        // Arrange
        var message = new Message
        {
            Id = "msg-no-headers",
            ThreadId = "thread-no-headers",
            Snippet = "No headers",
            Payload = new MessagePart
            {
                Headers = null,
                Body = null
            }
        };

        // Act
        var dto = GmailService.MapToDto(message);

        // Assert
        dto.Subject.Should().BeEmpty();
        dto.From.Should().BeEmpty();
        dto.To.Should().BeEmpty();
    }

    [Fact]
    public void MapToDto_DetectsAttachments()
    {
        // Arrange
        var message = new Message
        {
            Id = "msg-attach",
            ThreadId = "thread-attach",
            Snippet = "Has attachment",
            Payload = new MessagePart
            {
                Headers = new List<MessagePartHeader>(),
                Parts = new List<MessagePart>
                {
                    new()
                    {
                        MimeType = "text/plain",
                        Body = new MessagePartBody { Data = EncodeBase64Url("Some text") }
                    },
                    new()
                    {
                        MimeType = "application/pdf",
                        Filename = "document.pdf",
                        Body = new MessagePartBody { AttachmentId = "att-123" }
                    }
                }
            }
        };

        // Act
        var dto = GmailService.MapToDto(message);

        // Assert
        dto.HasAttachments.Should().BeTrue();
    }

    [Fact]
    public void MapToDto_NoAttachments_WhenNoFilenames()
    {
        // Arrange
        var message = new Message
        {
            Id = "msg-no-attach",
            ThreadId = "thread-no-attach",
            Snippet = "No attachment",
            Payload = new MessagePart
            {
                Headers = new List<MessagePartHeader>(),
                Parts = new List<MessagePart>
                {
                    new()
                    {
                        MimeType = "text/plain",
                        Body = new MessagePartBody { Data = EncodeBase64Url("Some text") }
                    }
                }
            }
        };

        // Act
        var dto = GmailService.MapToDto(message);

        // Assert
        dto.HasAttachments.Should().BeFalse();
    }

    [Fact]
    public void MapToDto_ExtractsPlainTextBody()
    {
        // Arrange
        var bodyText = "Hello, this is a plain text body.";
        var message = new Message
        {
            Id = "msg-text",
            ThreadId = "thread-text",
            Snippet = "Hello...",
            Payload = new MessagePart
            {
                Headers = new List<MessagePartHeader>(),
                MimeType = "multipart/alternative",
                Parts = new List<MessagePart>
                {
                    new()
                    {
                        MimeType = "text/plain",
                        Body = new MessagePartBody { Data = EncodeBase64Url(bodyText) }
                    },
                    new()
                    {
                        MimeType = "text/html",
                        Body = new MessagePartBody { Data = EncodeBase64Url("<p>Hello, this is HTML.</p>") }
                    }
                }
            }
        };

        // Act
        var dto = GmailService.MapToDto(message);

        // Assert
        dto.BodyPreview.Should().Be(bodyText);
    }

    [Fact]
    public void MapToDto_FallsBackToHtmlBody_WhenNoPlainText()
    {
        // Arrange
        var htmlBody = "<p>Hello HTML</p>";
        var message = new Message
        {
            Id = "msg-html",
            ThreadId = "thread-html",
            Snippet = "Hello...",
            Payload = new MessagePart
            {
                Headers = new List<MessagePartHeader>(),
                MimeType = "multipart/alternative",
                Parts = new List<MessagePart>
                {
                    new()
                    {
                        MimeType = "text/html",
                        Body = new MessagePartBody { Data = EncodeBase64Url(htmlBody) }
                    }
                }
            }
        };

        // Act
        var dto = GmailService.MapToDto(message);

        // Assert
        dto.BodyPreview.Should().Be(htmlBody);
    }

    [Fact]
    public void MapToDto_FallsBackToSnippet_WhenNoBody()
    {
        // Arrange
        var message = new Message
        {
            Id = "msg-snippet",
            ThreadId = "thread-snippet",
            Snippet = "Snippet fallback text",
            Payload = new MessagePart
            {
                Headers = new List<MessagePartHeader>(),
                MimeType = "multipart/alternative",
                Body = new MessagePartBody { Data = null },
                Parts = new List<MessagePart>()
            }
        };

        // Act
        var dto = GmailService.MapToDto(message);

        // Assert
        dto.BodyPreview.Should().Be("Snippet fallback text");
    }

    [Fact]
    public void MapToDto_HandlesInvalidDateHeader()
    {
        // Arrange
        var message = new Message
        {
            Id = "msg-bad-date",
            ThreadId = "thread-bad-date",
            Snippet = "Bad date",
            Payload = new MessagePart
            {
                Headers = new List<MessagePartHeader>
                {
                    new() { Name = "Date", Value = "not-a-date" }
                }
            }
        };

        // Act
        var dto = GmailService.MapToDto(message);

        // Assert
        dto.Date.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Theory]
    [InlineData("Hello World", "SGVsbG8gV29ybGQ")]
    [InlineData("Test with special chars: áéíóú", "VGVzdCB3aXRoIHNwZWNpYWwgY2hhcnM6IMOhw6nDrcOzw7o")]
    public void DecodeBase64Url_DecodesCorrectly(string expected, string input)
    {
        // Act
        var result = GmailService.DecodeBase64Url(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void CreateRawMessage_CreatesValidRfc2822Message()
    {
        // Act
        var raw = GmailService.CreateRawMessage("to@example.com", "Test Subject", "Test body");

        // Assert
        raw.Should().NotBeNullOrEmpty();

        // Decode and verify content
        var decoded = GmailService.DecodeBase64Url(raw);
        decoded.Should().Contain("To: to@example.com");
        decoded.Should().Contain("Subject: Test Subject");
        decoded.Should().Contain("Content-Type: text/plain; charset=utf-8");
        decoded.Should().Contain("Test body");
    }

    [Fact]
    public void CreateRawMessage_UsesBase64UrlEncoding()
    {
        // Act
        var raw = GmailService.CreateRawMessage("to@example.com", "Subject", "Body");

        // Assert — should not contain standard base64 characters that are URL-unsafe
        raw.Should().NotContain("+");
        raw.Should().NotContain("/");
        raw.Should().NotEndWith("=");
    }

    [Fact]
    public void Constructor_InitializesWithOptions()
    {
        // Act
        var service = new GmailService(_options, _logger);

        // Assert
        service.Should().NotBeNull();
    }

    private static string EncodeBase64Url(string text)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(text);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}
