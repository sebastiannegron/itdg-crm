namespace Itdg.Crm.Api.Test.Services;

using Itdg.Crm.Api.Infrastructure.Options;
using Itdg.Crm.Api.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using GoogleDriveFile = Google.Apis.Drive.v3.Data.File;

public class GoogleDriveServiceTests
{
    private readonly IOptions<GoogleDriveOptions> _options;
    private readonly ILogger<GoogleDriveService> _logger;

    public GoogleDriveServiceTests()
    {
        var driveOptions = new GoogleDriveOptions
        {
            ClientId = "test-client-id",
            ClientSecret = "test-client-secret",
            ApplicationName = "Test-App"
        };

        _options = Microsoft.Extensions.Options.Options.Create(driveOptions);
        _logger = Substitute.For<ILogger<GoogleDriveService>>();
    }

    [Fact]
    public void Constructor_InitializesWithOptions()
    {
        // Act
        var service = new GoogleDriveService(_options, _logger);

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void MapToDto_MapsBasicFieldsCorrectly()
    {
        // Arrange
        var file = new GoogleDriveFile
        {
            Id = "file-123",
            Name = "test-document.pdf",
            MimeType = "application/pdf",
            Size = 1024,
            CreatedTimeDateTimeOffset = new DateTimeOffset(2025, 6, 15, 10, 30, 0, TimeSpan.Zero),
            ModifiedTimeDateTimeOffset = new DateTimeOffset(2025, 6, 16, 14, 0, 0, TimeSpan.Zero),
            WebViewLink = "https://drive.google.com/file/d/file-123/view",
            Parents = new List<string> { "folder-456" }
        };

        // Act
        var dto = GoogleDriveService.MapToDto(file);

        // Assert
        dto.Id.Should().Be("file-123");
        dto.Name.Should().Be("test-document.pdf");
        dto.MimeType.Should().Be("application/pdf");
        dto.Size.Should().Be(1024);
        dto.CreatedTime.Should().Be(new DateTimeOffset(2025, 6, 15, 10, 30, 0, TimeSpan.Zero));
        dto.ModifiedTime.Should().Be(new DateTimeOffset(2025, 6, 16, 14, 0, 0, TimeSpan.Zero));
        dto.WebViewLink.Should().Be("https://drive.google.com/file/d/file-123/view");
        dto.Parents.Should().BeEquivalentTo(new[] { "folder-456" });
    }

    [Fact]
    public void MapToDto_HandlesNullParents()
    {
        // Arrange
        var file = new GoogleDriveFile
        {
            Id = "file-no-parents",
            Name = "orphan.txt",
            MimeType = "text/plain",
            Parents = null
        };

        // Act
        var dto = GoogleDriveService.MapToDto(file);

        // Assert
        dto.Id.Should().Be("file-no-parents");
        dto.Parents.Should().BeEmpty();
    }

    [Fact]
    public void MapToDto_HandlesNullOptionalFields()
    {
        // Arrange
        var file = new GoogleDriveFile
        {
            Id = "file-minimal",
            Name = "minimal.txt",
            MimeType = "text/plain",
            Size = null,
            CreatedTimeDateTimeOffset = null,
            ModifiedTimeDateTimeOffset = null,
            WebViewLink = null,
            Parents = null
        };

        // Act
        var dto = GoogleDriveService.MapToDto(file);

        // Assert
        dto.Id.Should().Be("file-minimal");
        dto.Name.Should().Be("minimal.txt");
        dto.MimeType.Should().Be("text/plain");
        dto.Size.Should().BeNull();
        dto.CreatedTime.Should().BeNull();
        dto.ModifiedTime.Should().BeNull();
        dto.WebViewLink.Should().BeNull();
        dto.Parents.Should().BeEmpty();
    }

    [Fact]
    public void MapToDto_MapsFolderCorrectly()
    {
        // Arrange
        var folder = new GoogleDriveFile
        {
            Id = "folder-789",
            Name = "Client Documents",
            MimeType = "application/vnd.google-apps.folder",
            Size = null,
            CreatedTimeDateTimeOffset = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero),
            ModifiedTimeDateTimeOffset = new DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero),
            WebViewLink = "https://drive.google.com/drive/folders/folder-789",
            Parents = new List<string> { "root" }
        };

        // Act
        var dto = GoogleDriveService.MapToDto(folder);

        // Assert
        dto.Id.Should().Be("folder-789");
        dto.Name.Should().Be("Client Documents");
        dto.MimeType.Should().Be("application/vnd.google-apps.folder");
        dto.Parents.Should().BeEquivalentTo(new[] { "root" });
    }

    [Fact]
    public void MapToDto_HandlesMultipleParents()
    {
        // Arrange
        var file = new GoogleDriveFile
        {
            Id = "file-multi",
            Name = "shared.pdf",
            MimeType = "application/pdf",
            Parents = new List<string> { "folder-1", "folder-2" }
        };

        // Act
        var dto = GoogleDriveService.MapToDto(file);

        // Assert
        dto.Parents.Should().HaveCount(2);
        dto.Parents.Should().BeEquivalentTo(new[] { "folder-1", "folder-2" });
    }

    [Fact]
    public void CreateDriveClient_ReturnsValidService()
    {
        // Arrange
        var service = new GoogleDriveService(_options, _logger);

        // Act
        using var driveClient = service.CreateDriveClient("test-access-token");

        // Assert
        driveClient.Should().NotBeNull();
        driveClient.ApplicationName.Should().Be("Test-App");
    }
}
