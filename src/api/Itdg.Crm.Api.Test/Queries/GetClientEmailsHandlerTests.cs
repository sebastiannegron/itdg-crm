namespace Itdg.Crm.Api.Test.Queries;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Application.QueryHandlers;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetClientEmailsHandlerTests
{
    private readonly IEmailMirrorRepository _emailMirrorRepository;
    private readonly IUserRepository _userRepository;
    private readonly IClientAssignmentRepository _clientAssignmentRepository;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ILogger<GetClientEmailsHandler> _logger;
    private readonly GetClientEmailsHandler _handler;
    private readonly Guid _clientId = Guid.NewGuid();

    public GetClientEmailsHandlerTests()
    {
        _emailMirrorRepository = Substitute.For<IEmailMirrorRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _clientAssignmentRepository = Substitute.For<IClientAssignmentRepository>();
        _currentUserProvider = Substitute.For<ICurrentUserProvider>();
        _logger = Substitute.For<ILogger<GetClientEmailsHandler>>();

        _handler = new GetClientEmailsHandler(
            _emailMirrorRepository, _userRepository, _clientAssignmentRepository,
            _currentUserProvider, _logger);

        // Default: Administrator (sees all, no assignment check)
        _currentUserProvider.IsInRole(nameof(UserRole.Administrator)).Returns(true);
    }

    [Fact]
    public async Task HandleAsync_ReturnsPaginatedResult_WithCorrectData()
    {
        // Arrange
        var emails = new List<EmailMirror>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ClientId = _clientId,
                GmailMessageId = "msg-1",
                GmailThreadId = "thread-1",
                Subject = "Tax Return Discussion",
                From = "client@example.com",
                To = "team@example.com",
                BodyPreview = "Hello, I wanted to discuss...",
                HasAttachments = false,
                ReceivedAt = DateTimeOffset.UtcNow.AddHours(-1),
                TenantId = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                ClientId = _clientId,
                GmailMessageId = "msg-2",
                GmailThreadId = "thread-1",
                Subject = "Re: Tax Return Discussion",
                From = "team@example.com",
                To = "client@example.com",
                BodyPreview = "Sure, let me check...",
                HasAttachments = true,
                ReceivedAt = DateTimeOffset.UtcNow,
                TenantId = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow
            }
        };

        _emailMirrorRepository.GetPagedByClientIdAsync(
            _clientId, 1, 20, null, Arg.Any<CancellationToken>())
            .Returns((emails.AsReadOnly(), 2));

        var query = new GetClientEmails(_clientId);

        // Act
        var result = await _handler.HandleAsync(query, Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task HandleAsync_MapsEmailProperties_Correctly()
    {
        // Arrange
        var emailId = Guid.NewGuid();
        var receivedAt = DateTimeOffset.UtcNow.AddDays(-1);

        var email = new EmailMirror
        {
            Id = emailId,
            ClientId = _clientId,
            GmailMessageId = "msg-123",
            GmailThreadId = "thread-456",
            Subject = "Quarterly Report",
            From = "client@example.com",
            To = "team@example.com",
            BodyPreview = "Please find attached...",
            HasAttachments = true,
            ReceivedAt = receivedAt,
            TenantId = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _emailMirrorRepository.GetPagedByClientIdAsync(
            _clientId, 1, 20, null, Arg.Any<CancellationToken>())
            .Returns((new List<EmailMirror> { email }.AsReadOnly(), 1));

        // Act
        var result = await _handler.HandleAsync(new GetClientEmails(_clientId), Guid.NewGuid(), CancellationToken.None);

        // Assert
        var dto = result.Items.First();
        dto.EmailId.Should().Be(emailId);
        dto.ClientId.Should().Be(_clientId);
        dto.GmailMessageId.Should().Be("msg-123");
        dto.GmailThreadId.Should().Be("thread-456");
        dto.Subject.Should().Be("Quarterly Report");
        dto.From.Should().Be("client@example.com");
        dto.To.Should().Be("team@example.com");
        dto.BodyPreview.Should().Be("Please find attached...");
        dto.HasAttachments.Should().BeTrue();
        dto.ReceivedAt.Should().Be(receivedAt);
    }

    [Fact]
    public async Task HandleAsync_PassesSearchFilter_ToRepository()
    {
        // Arrange
        _emailMirrorRepository.GetPagedByClientIdAsync(
            _clientId, 2, 10, "tax", Arg.Any<CancellationToken>())
            .Returns((new List<EmailMirror>().AsReadOnly(), 0));

        var query = new GetClientEmails(_clientId, Page: 2, PageSize: 10, Search: "tax");

        // Act
        var result = await _handler.HandleAsync(query, Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _emailMirrorRepository.Received(1).GetPagedByClientIdAsync(
            _clientId, 2, 10, "tax", Arg.Any<CancellationToken>());
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task HandleAsync_ReturnsEmptyResult_WhenNoEmails()
    {
        // Arrange
        _emailMirrorRepository.GetPagedByClientIdAsync(
            _clientId, 1, 20, null, Arg.Any<CancellationToken>())
            .Returns((new List<EmailMirror>().AsReadOnly(), 0));

        // Act
        var result = await _handler.HandleAsync(new GetClientEmails(_clientId), Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task HandleAsync_Administrator_SkipsAssignmentCheck()
    {
        // Arrange
        _currentUserProvider.IsInRole(nameof(UserRole.Administrator)).Returns(true);

        _emailMirrorRepository.GetPagedByClientIdAsync(
            _clientId, 1, 20, null, Arg.Any<CancellationToken>())
            .Returns((new List<EmailMirror>().AsReadOnly(), 0));

        // Act
        await _handler.HandleAsync(new GetClientEmails(_clientId), Guid.NewGuid(), CancellationToken.None);

        // Assert
        await _clientAssignmentRepository.DidNotReceive().ExistsAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _userRepository.DidNotReceive().GetByEntraObjectIdAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_Associate_Succeeds_WhenAssignedToClient()
    {
        // Arrange
        var entraObjectId = "test-entra-id";
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            EntraObjectId = entraObjectId,
            Email = "associate@example.com",
            DisplayName = "Associate User",
            Role = UserRole.Associate,
            TenantId = Guid.NewGuid()
        };

        _currentUserProvider.IsInRole(nameof(UserRole.Administrator)).Returns(false);
        _currentUserProvider.GetEntraObjectId().Returns(entraObjectId);
        _userRepository.GetByEntraObjectIdAsync(entraObjectId, Arg.Any<CancellationToken>()).Returns(user);
        _clientAssignmentRepository.ExistsAsync(userId, _clientId, Arg.Any<CancellationToken>()).Returns(true);

        _emailMirrorRepository.GetPagedByClientIdAsync(
            _clientId, 1, 20, null, Arg.Any<CancellationToken>())
            .Returns((new List<EmailMirror>().AsReadOnly(), 0));

        // Act
        var result = await _handler.HandleAsync(new GetClientEmails(_clientId), Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        await _clientAssignmentRepository.Received(1).ExistsAsync(userId, _clientId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_Associate_ThrowsForbidden_WhenNotAssignedToClient()
    {
        // Arrange
        var entraObjectId = "test-entra-id";
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            EntraObjectId = entraObjectId,
            Email = "associate@example.com",
            DisplayName = "Associate User",
            Role = UserRole.Associate,
            TenantId = Guid.NewGuid()
        };

        _currentUserProvider.IsInRole(nameof(UserRole.Administrator)).Returns(false);
        _currentUserProvider.GetEntraObjectId().Returns(entraObjectId);
        _userRepository.GetByEntraObjectIdAsync(entraObjectId, Arg.Any<CancellationToken>()).Returns(user);
        _clientAssignmentRepository.ExistsAsync(userId, _clientId, Arg.Any<CancellationToken>()).Returns(false);

        // Act
        var act = () => _handler.HandleAsync(new GetClientEmails(_clientId), Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task HandleAsync_Associate_ThrowsForbidden_WhenUserNotFound()
    {
        // Arrange
        _currentUserProvider.IsInRole(nameof(UserRole.Administrator)).Returns(false);
        _currentUserProvider.GetEntraObjectId().Returns("unknown-entra-id");
        _userRepository.GetByEntraObjectIdAsync("unknown-entra-id", Arg.Any<CancellationToken>()).Returns((User?)null);

        // Act
        var act = () => _handler.HandleAsync(new GetClientEmails(_clientId), Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task HandleAsync_Associate_ThrowsForbidden_WhenEntraObjectIdMissing()
    {
        // Arrange
        _currentUserProvider.IsInRole(nameof(UserRole.Administrator)).Returns(false);
        _currentUserProvider.GetEntraObjectId().Returns((string?)null);

        // Act
        var act = () => _handler.HandleAsync(new GetClientEmails(_clientId), Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
