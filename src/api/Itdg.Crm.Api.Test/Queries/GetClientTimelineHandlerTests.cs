namespace Itdg.Crm.Api.Test.Queries;

using Itdg.Crm.Api.Application.Abstractions;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Application.Exceptions;
using Itdg.Crm.Api.Application.Queries;
using Itdg.Crm.Api.Application.QueryHandlers;
using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.Enums;
using Itdg.Crm.Api.Domain.GeneralConstants;
using Itdg.Crm.Api.Domain.Repositories;
using Microsoft.Extensions.Logging;

public class GetClientTimelineHandlerTests
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IEmailMirrorRepository _emailMirrorRepository;
    private readonly IUserRepository _userRepository;
    private readonly IClientAssignmentRepository _clientAssignmentRepository;
    private readonly ICurrentUserProvider _currentUserProvider;
    private readonly ILogger<GetClientTimelineHandler> _logger;
    private readonly GetClientTimelineHandler _handler;
    private readonly Guid _clientId = Guid.NewGuid();

    public GetClientTimelineHandlerTests()
    {
        _documentRepository = Substitute.For<IDocumentRepository>();
        _messageRepository = Substitute.For<IMessageRepository>();
        _emailMirrorRepository = Substitute.For<IEmailMirrorRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _clientAssignmentRepository = Substitute.For<IClientAssignmentRepository>();
        _currentUserProvider = Substitute.For<ICurrentUserProvider>();
        _logger = Substitute.For<ILogger<GetClientTimelineHandler>>();

        _handler = new GetClientTimelineHandler(
            _documentRepository, _messageRepository, _emailMirrorRepository,
            _userRepository, _clientAssignmentRepository,
            _currentUserProvider, _logger);

        // Default: Administrator (sees all, no assignment check)
        _currentUserProvider.IsInRole(nameof(UserRole.Administrator)).Returns(true);

        // Default: empty results
        _documentRepository.GetByClientIdAsync(_clientId, Arg.Any<CancellationToken>())
            .Returns(new List<Document>().AsReadOnly());
        _messageRepository.GetByClientIdAsync(_clientId, Arg.Any<CancellationToken>())
            .Returns(new List<Message>().AsReadOnly());
        _emailMirrorRepository.GetByClientIdAsync(_clientId, Arg.Any<CancellationToken>())
            .Returns(new List<EmailMirror>().AsReadOnly());
    }

    [Fact]
    public async Task HandleAsync_ReturnsEmptyResult_WhenNoActivity()
    {
        // Act
        var result = await _handler.HandleAsync(new GetClientTimeline(_clientId), Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task HandleAsync_AggregatesDocuments_IntoTimeline()
    {
        // Arrange
        var docId = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow.AddDays(-1);
        var documents = new List<Document>
        {
            new()
            {
                Id = docId,
                ClientId = _clientId,
                CategoryId = Guid.NewGuid(),
                FileName = "tax-return.pdf",
                GoogleDriveFileId = "drive-1",
                UploadedById = Guid.NewGuid(),
                CurrentVersion = 1,
                FileSize = 1024,
                MimeType = "application/pdf",
                TenantId = Guid.NewGuid(),
                CreatedAt = createdAt,
                UpdatedAt = createdAt
            }
        };

        _documentRepository.GetByClientIdAsync(_clientId, Arg.Any<CancellationToken>())
            .Returns(documents.AsReadOnly());

        // Act
        var result = await _handler.HandleAsync(new GetClientTimeline(_clientId), Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        var item = result.Items.First();
        item.Id.Should().Be(docId);
        item.Type.Should().Be("document");
        item.Description.Should().Be("tax-return.pdf");
        item.Timestamp.Should().Be(createdAt);
    }

    [Fact]
    public async Task HandleAsync_AggregatesMessages_IntoTimeline()
    {
        // Arrange
        var msgId = Guid.NewGuid();
        var createdAt = DateTimeOffset.UtcNow.AddHours(-2);
        var messages = new List<Message>
        {
            new()
            {
                Id = msgId,
                ClientId = _clientId,
                SenderId = Guid.NewGuid(),
                Direction = MessageDirection.Outbound,
                Subject = "Tax consultation update",
                Body = "Body text",
                TenantId = Guid.NewGuid(),
                CreatedAt = createdAt,
                UpdatedAt = createdAt
            }
        };

        _messageRepository.GetByClientIdAsync(_clientId, Arg.Any<CancellationToken>())
            .Returns(messages.AsReadOnly());

        // Act
        var result = await _handler.HandleAsync(new GetClientTimeline(_clientId), Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        var item = result.Items.First();
        item.Id.Should().Be(msgId);
        item.Type.Should().Be("message");
        item.Description.Should().Be("Tax consultation update");
        item.Timestamp.Should().Be(createdAt);
    }

    [Fact]
    public async Task HandleAsync_AggregatesEmails_IntoTimeline()
    {
        // Arrange
        var emailId = Guid.NewGuid();
        var receivedAt = DateTimeOffset.UtcNow.AddHours(-3);
        var emails = new List<EmailMirror>
        {
            new()
            {
                Id = emailId,
                ClientId = _clientId,
                GmailMessageId = "gmail-1",
                GmailThreadId = "thread-1",
                Subject = "RE: Filing deadline",
                From = "client@example.com",
                To = "firm@example.com",
                ReceivedAt = receivedAt,
                TenantId = Guid.NewGuid(),
                CreatedAt = receivedAt,
                UpdatedAt = receivedAt
            }
        };

        _emailMirrorRepository.GetByClientIdAsync(_clientId, Arg.Any<CancellationToken>())
            .Returns(emails.AsReadOnly());

        // Act
        var result = await _handler.HandleAsync(new GetClientTimeline(_clientId), Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        var item = result.Items.First();
        item.Id.Should().Be(emailId);
        item.Type.Should().Be("email");
        item.Description.Should().Be("RE: Filing deadline");
        item.Timestamp.Should().Be(receivedAt);
        item.Actor.Should().Be("client@example.com");
    }

    [Fact]
    public async Task HandleAsync_SortsByTimestamp_Descending()
    {
        // Arrange
        var oldest = DateTimeOffset.UtcNow.AddDays(-3);
        var middle = DateTimeOffset.UtcNow.AddDays(-2);
        var newest = DateTimeOffset.UtcNow.AddDays(-1);

        var documents = new List<Document>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ClientId = _clientId,
                CategoryId = Guid.NewGuid(),
                FileName = "oldest-doc.pdf",
                GoogleDriveFileId = "drive-1",
                UploadedById = Guid.NewGuid(),
                CurrentVersion = 1,
                FileSize = 1024,
                MimeType = "application/pdf",
                TenantId = Guid.NewGuid(),
                CreatedAt = oldest,
                UpdatedAt = oldest
            }
        };

        var messages = new List<Message>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ClientId = _clientId,
                SenderId = Guid.NewGuid(),
                Direction = MessageDirection.Outbound,
                Subject = "newest-message",
                Body = "Body",
                TenantId = Guid.NewGuid(),
                CreatedAt = newest,
                UpdatedAt = newest
            }
        };

        var emails = new List<EmailMirror>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ClientId = _clientId,
                GmailMessageId = "gmail-1",
                GmailThreadId = "thread-1",
                Subject = "middle-email",
                From = "test@example.com",
                To = "firm@example.com",
                ReceivedAt = middle,
                TenantId = Guid.NewGuid(),
                CreatedAt = middle,
                UpdatedAt = middle
            }
        };

        _documentRepository.GetByClientIdAsync(_clientId, Arg.Any<CancellationToken>())
            .Returns(documents.AsReadOnly());
        _messageRepository.GetByClientIdAsync(_clientId, Arg.Any<CancellationToken>())
            .Returns(messages.AsReadOnly());
        _emailMirrorRepository.GetByClientIdAsync(_clientId, Arg.Any<CancellationToken>())
            .Returns(emails.AsReadOnly());

        // Act
        var result = await _handler.HandleAsync(new GetClientTimeline(_clientId), Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(3);
        result.Items[0].Description.Should().Be("newest-message");
        result.Items[1].Description.Should().Be("middle-email");
        result.Items[2].Description.Should().Be("oldest-doc.pdf");
    }

    [Fact]
    public async Task HandleAsync_Paginates_Correctly()
    {
        // Arrange
        var documents = Enumerable.Range(1, 25).Select(i => new Document
        {
            Id = Guid.NewGuid(),
            ClientId = _clientId,
            CategoryId = Guid.NewGuid(),
            FileName = $"doc-{i}.pdf",
            GoogleDriveFileId = $"drive-{i}",
            UploadedById = Guid.NewGuid(),
            CurrentVersion = 1,
            FileSize = 1024,
            MimeType = "application/pdf",
            TenantId = Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow.AddHours(-i),
            UpdatedAt = DateTimeOffset.UtcNow.AddHours(-i)
        }).ToList();

        _documentRepository.GetByClientIdAsync(_clientId, Arg.Any<CancellationToken>())
            .Returns(documents.AsReadOnly());

        var query = new GetClientTimeline(_clientId, Page: 2, PageSize: 10);

        // Act
        var result = await _handler.HandleAsync(query, Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(25);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(10);
        result.Items.Should().HaveCount(10);
    }

    [Fact]
    public async Task HandleAsync_Administrator_SkipsAssignmentCheck()
    {
        // Arrange
        _currentUserProvider.IsInRole(nameof(UserRole.Administrator)).Returns(true);

        // Act
        await _handler.HandleAsync(new GetClientTimeline(_clientId), Guid.NewGuid(), CancellationToken.None);

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

        // Act
        var result = await _handler.HandleAsync(new GetClientTimeline(_clientId), Guid.NewGuid(), CancellationToken.None);

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
        var act = () => _handler.HandleAsync(new GetClientTimeline(_clientId), Guid.NewGuid(), CancellationToken.None);

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
        var act = () => _handler.HandleAsync(new GetClientTimeline(_clientId), Guid.NewGuid(), CancellationToken.None);

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
        var act = () => _handler.HandleAsync(new GetClientTimeline(_clientId), Guid.NewGuid(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task HandleAsync_ReturnsTotalCount_AcrossAllTypes()
    {
        // Arrange
        var documents = new List<Document>
        {
            new()
            {
                Id = Guid.NewGuid(), ClientId = _clientId, CategoryId = Guid.NewGuid(),
                FileName = "doc.pdf", GoogleDriveFileId = "drive-1", UploadedById = Guid.NewGuid(),
                CurrentVersion = 1, FileSize = 1024, MimeType = "application/pdf",
                TenantId = Guid.NewGuid(), CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow
            }
        };

        var messages = new List<Message>
        {
            new()
            {
                Id = Guid.NewGuid(), ClientId = _clientId, SenderId = Guid.NewGuid(),
                Direction = MessageDirection.Outbound, Subject = "msg", Body = "body",
                TenantId = Guid.NewGuid(), CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow
            }
        };

        var emails = new List<EmailMirror>
        {
            new()
            {
                Id = Guid.NewGuid(), ClientId = _clientId, GmailMessageId = "g1",
                GmailThreadId = "t1", Subject = "email", From = "a@b.com", To = "c@d.com",
                ReceivedAt = DateTimeOffset.UtcNow, TenantId = Guid.NewGuid(),
                CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow
            }
        };

        _documentRepository.GetByClientIdAsync(_clientId, Arg.Any<CancellationToken>())
            .Returns(documents.AsReadOnly());
        _messageRepository.GetByClientIdAsync(_clientId, Arg.Any<CancellationToken>())
            .Returns(messages.AsReadOnly());
        _emailMirrorRepository.GetByClientIdAsync(_clientId, Arg.Any<CancellationToken>())
            .Returns(emails.AsReadOnly());

        // Act
        var result = await _handler.HandleAsync(new GetClientTimeline(_clientId), Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.TotalCount.Should().Be(3);
        result.Items.Should().HaveCount(3);
    }
}
