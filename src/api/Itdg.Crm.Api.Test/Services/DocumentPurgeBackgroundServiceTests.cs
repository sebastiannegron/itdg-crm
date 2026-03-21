namespace Itdg.Crm.Api.Test.Services;

using Itdg.Crm.Api.Domain.Entities;
using Itdg.Crm.Api.Domain.Repositories;
using Itdg.Crm.Api.Infrastructure.Options;
using Itdg.Crm.Api.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class DocumentPurgeBackgroundServiceTests
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IDocumentRepository _documentRepository;
    private readonly ILogger<DocumentPurgeBackgroundService> _logger;

    public DocumentPurgeBackgroundServiceTests()
    {
        _documentRepository = Substitute.For<IDocumentRepository>();
        _logger = Substitute.For<ILogger<DocumentPurgeBackgroundService>>();
        _serviceProvider = Substitute.For<IServiceProvider>();
    }

    private DocumentPurgeBackgroundService CreateService(DocumentPurgeOptions? purgeOptions = null)
    {
        var options = Microsoft.Extensions.Options.Options.Create(
            purgeOptions ?? new DocumentPurgeOptions());

        return new DocumentPurgeBackgroundService(_serviceProvider, options, _logger);
    }

    private void SetupServiceProvider()
    {
        var scopeServiceProvider = Substitute.For<IServiceProvider>();
        scopeServiceProvider.GetService(typeof(IDocumentRepository)).Returns(_documentRepository);

        var scope = Substitute.For<IServiceScope>();
        scope.ServiceProvider.Returns(scopeServiceProvider);

        var scopeFactory = Substitute.For<IServiceScopeFactory>();
        scopeFactory.CreateScope().Returns(scope);

        _serviceProvider.GetService(typeof(IServiceScopeFactory)).Returns(scopeFactory);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDisabled_DoesNotRun()
    {
        // Arrange
        var options = new DocumentPurgeOptions { Enabled = false };
        var service = CreateService(options);

        // Act
        await service.StartAsync(CancellationToken.None);
        await Task.Delay(100);
        await service.StopAsync(CancellationToken.None);

        // Assert
        _logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("disabled")),
            Arg.Any<Exception?>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task PurgeDeletedDocumentsAsync_WhenNoExpiredDocuments_LogsAndReturns()
    {
        // Arrange
        SetupServiceProvider();

        _documentRepository.GetDocumentsDeletedBeforeAsync(Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(new List<Document>().AsReadOnly());

        var service = CreateService();

        // Act
        await service.PurgeDeletedDocumentsAsync(CancellationToken.None);

        // Assert
        await _documentRepository.DidNotReceive().DeleteAsync(Arg.Any<Document>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PurgeDeletedDocumentsAsync_PermanentlyDeletesExpiredDocuments()
    {
        // Arrange
        SetupServiceProvider();

        var expiredDoc = new Document
        {
            Id = Guid.NewGuid(),
            ClientId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            FileName = "expired.pdf",
            GoogleDriveFileId = "drive-id",
            UploadedById = Guid.NewGuid(),
            CurrentVersion = 1,
            FileSize = 1024,
            MimeType = "application/pdf",
            TenantId = Guid.NewGuid(),
            DeletedAt = DateTimeOffset.UtcNow.AddDays(-45)
        };

        _documentRepository.GetDocumentsDeletedBeforeAsync(Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(new List<Document> { expiredDoc }.AsReadOnly());

        var service = CreateService();

        // Act
        await service.PurgeDeletedDocumentsAsync(CancellationToken.None);

        // Assert
        await _documentRepository.Received(1).DeleteAsync(expiredDoc, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PurgeDeletedDocumentsAsync_UsesConfiguredRetentionDays()
    {
        // Arrange
        SetupServiceProvider();

        _documentRepository.GetDocumentsDeletedBeforeAsync(Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(new List<Document>().AsReadOnly());

        var options = new DocumentPurgeOptions { RetentionDays = 60 };
        var service = CreateService(options);

        // Act
        await service.PurgeDeletedDocumentsAsync(CancellationToken.None);

        // Assert
        await _documentRepository.Received(1).GetDocumentsDeletedBeforeAsync(
            Arg.Is<DateTimeOffset>(d => d < DateTimeOffset.UtcNow.AddDays(-59) && d > DateTimeOffset.UtcNow.AddDays(-61)),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PurgeDeletedDocumentsAsync_ContinuesOnFailure_ForIndividualDocuments()
    {
        // Arrange
        SetupServiceProvider();

        var doc1 = new Document
        {
            Id = Guid.NewGuid(),
            ClientId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            FileName = "fail.pdf",
            GoogleDriveFileId = "drive-id-1",
            UploadedById = Guid.NewGuid(),
            CurrentVersion = 1,
            FileSize = 1024,
            MimeType = "application/pdf",
            TenantId = Guid.NewGuid(),
            DeletedAt = DateTimeOffset.UtcNow.AddDays(-45)
        };

        var doc2 = new Document
        {
            Id = Guid.NewGuid(),
            ClientId = Guid.NewGuid(),
            CategoryId = Guid.NewGuid(),
            FileName = "success.pdf",
            GoogleDriveFileId = "drive-id-2",
            UploadedById = Guid.NewGuid(),
            CurrentVersion = 1,
            FileSize = 2048,
            MimeType = "application/pdf",
            TenantId = Guid.NewGuid(),
            DeletedAt = DateTimeOffset.UtcNow.AddDays(-45)
        };

        _documentRepository.GetDocumentsDeletedBeforeAsync(Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(new List<Document> { doc1, doc2 }.AsReadOnly());

        _documentRepository.DeleteAsync(doc1, Arg.Any<CancellationToken>())
            .Returns<Task>(x => throw new Exception("Database error"));

        var service = CreateService();

        // Act — should not throw
        await service.PurgeDeletedDocumentsAsync(CancellationToken.None);

        // Assert — both should have been attempted
        await _documentRepository.Received(1).DeleteAsync(doc1, Arg.Any<CancellationToken>());
        await _documentRepository.Received(1).DeleteAsync(doc2, Arg.Any<CancellationToken>());
    }
}
