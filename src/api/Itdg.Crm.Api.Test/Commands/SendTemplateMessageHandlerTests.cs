namespace Itdg.Crm.Api.Test.Commands;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.CommandHandlers;
using Itdg.Crm.Api.Application.Commands;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.Enums;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class SendTemplateMessageHandlerTests
{
    private readonly ITemplateRepository _templateRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly ITemplateRenderer _renderer;
    private readonly IEmailSender _emailSender;
    private readonly ILogger<SendTemplateMessageHandler> _logger;
    private readonly SendTemplateMessageHandler _handler;

    public SendTemplateMessageHandlerTests()
    {
        _templateRepository = Substitute.For<ITemplateRepository>();
        _messageRepository = Substitute.For<IMessageRepository>();
        _renderer = Substitute.For<ITemplateRenderer>();
        _emailSender = Substitute.For<IEmailSender>();
        _logger = Substitute.For<ILogger<SendTemplateMessageHandler>>();
        _handler = new SendTemplateMessageHandler(
            _templateRepository, _messageRepository, _renderer, _emailSender, _logger);
    }

    [Fact]
    public async Task HandleAsync_CreatesPortalMessage_WhenSendViaPortalIsTrue()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var senderId = Guid.NewGuid();
        var mergeFields = new Dictionary<string, string> { { "client_name", "John Doe" } };

        var template = new CommunicationTemplate
        {
            Id = templateId,
            Name = "Welcome Template",
            SubjectTemplate = "Welcome {{client_name}}",
            BodyTemplate = "Hello {{client_name}}, welcome!",
            Language = "en-pr",
            Category = TemplateCategory.Onboarding,
            Version = 1,
            IsActive = true,
            CreatedById = Guid.NewGuid()
        };

        _templateRepository.GetByIdAsync(templateId, Arg.Any<CancellationToken>())
            .Returns(template);
        _renderer.Render("Welcome {{client_name}}", Arg.Any<IDictionary<string, string>>())
            .Returns("Welcome John Doe");
        _renderer.Render("Hello {{client_name}}, welcome!", Arg.Any<IDictionary<string, string>>())
            .Returns("Hello John Doe, welcome!");

        var command = new SendTemplateMessage(
            templateId, clientId, senderId, mergeFields,
            SendViaPortal: true, SendViaEmail: false, RecipientEmail: null);

        Message? capturedMessage = null;
        _messageRepository.AddAsync(Arg.Do<Message>(m => capturedMessage = m), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _messageRepository.Received(1).AddAsync(Arg.Any<Message>(), Arg.Any<CancellationToken>());
        capturedMessage.Should().NotBeNull();
        capturedMessage!.ClientId.Should().Be(clientId);
        capturedMessage.SenderId.Should().Be(senderId);
        capturedMessage.Subject.Should().Be("Welcome John Doe");
        capturedMessage.Body.Should().Be("Hello John Doe, welcome!");
        capturedMessage.Direction.Should().Be(MessageDirection.Outbound);
        capturedMessage.IsPortalMessage.Should().BeTrue();
        capturedMessage.IsRead.Should().BeFalse();
        capturedMessage.TemplateId.Should().Be(templateId);

        await _emailSender.DidNotReceive().SendAsync(
            Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_SendsEmail_WhenSendViaEmailIsTrue()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var senderId = Guid.NewGuid();
        var mergeFields = new Dictionary<string, string> { { "client_name", "Jane" } };

        var template = new CommunicationTemplate
        {
            Id = templateId,
            Name = "Reminder",
            SubjectTemplate = "Reminder for {{client_name}}",
            BodyTemplate = "Dear {{client_name}}, please review.",
            Language = "en-pr",
            Category = TemplateCategory.PaymentReminder,
            Version = 1,
            IsActive = true,
            CreatedById = Guid.NewGuid()
        };

        _templateRepository.GetByIdAsync(templateId, Arg.Any<CancellationToken>())
            .Returns(template);
        _renderer.Render(template.SubjectTemplate, Arg.Any<IDictionary<string, string>>())
            .Returns("Reminder for Jane");
        _renderer.Render(template.BodyTemplate, Arg.Any<IDictionary<string, string>>())
            .Returns("Dear Jane, please review.");

        var command = new SendTemplateMessage(
            templateId, clientId, senderId, mergeFields,
            SendViaPortal: false, SendViaEmail: true, RecipientEmail: "jane@example.com");

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _messageRepository.DidNotReceive().AddAsync(Arg.Any<Message>(), Arg.Any<CancellationToken>());
        await _emailSender.Received(1).SendAsync(
            "jane@example.com", "Reminder for Jane", "Dear Jane, please review.", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_SendsBothPortalAndEmail_WhenBothEnabled()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var clientId = Guid.NewGuid();
        var senderId = Guid.NewGuid();
        var mergeFields = new Dictionary<string, string> { { "client_name", "Bob" } };

        var template = new CommunicationTemplate
        {
            Id = templateId,
            Name = "Tax Season",
            SubjectTemplate = "Tax Season {{client_name}}",
            BodyTemplate = "Hello {{client_name}}, tax season is here.",
            Language = "en-pr",
            Category = TemplateCategory.TaxSeason,
            Version = 1,
            IsActive = true,
            CreatedById = Guid.NewGuid()
        };

        _templateRepository.GetByIdAsync(templateId, Arg.Any<CancellationToken>())
            .Returns(template);
        _renderer.Render(template.SubjectTemplate, Arg.Any<IDictionary<string, string>>())
            .Returns("Tax Season Bob");
        _renderer.Render(template.BodyTemplate, Arg.Any<IDictionary<string, string>>())
            .Returns("Hello Bob, tax season is here.");

        var command = new SendTemplateMessage(
            templateId, clientId, senderId, mergeFields,
            SendViaPortal: true, SendViaEmail: true, RecipientEmail: "bob@example.com");

        // Act
        await _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _messageRepository.Received(1).AddAsync(Arg.Any<Message>(), Arg.Any<CancellationToken>());
        await _emailSender.Received(1).SendAsync(
            "bob@example.com", "Tax Season Bob", "Hello Bob, tax season is here.", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_ThrowsNotFoundException_WhenTemplateNotFound()
    {
        // Arrange
        var templateId = Guid.NewGuid();
        var mergeFields = new Dictionary<string, string>();

        _templateRepository.GetByIdAsync(templateId, Arg.Any<CancellationToken>())
            .Returns((CommunicationTemplate?)null);

        var command = new SendTemplateMessage(
            templateId, Guid.NewGuid(), Guid.NewGuid(), mergeFields,
            SendViaPortal: true, SendViaEmail: false, RecipientEmail: null);

        // Act & Assert
        await FluentActions.Invoking(() =>
            _handler.HandleAsync(command, "en-pr", Guid.NewGuid(), CancellationToken.None))
            .Should().ThrowAsync<NotFoundException>();
    }
}
