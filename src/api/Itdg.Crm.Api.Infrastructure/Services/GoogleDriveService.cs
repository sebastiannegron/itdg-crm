namespace Itdg.Crm.Api.Infrastructure.Services;

using System.Diagnostics;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using Itdg.Crm.Api.Application.Dtos;
using Itdg.Crm.Api.Diagnostics;
using Itdg.Crm.Api.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using GoogleDriveFile = Google.Apis.Drive.v3.Data.File;

public class GoogleDriveService : IGoogleDriveService
{
    private const string FolderMimeType = "application/vnd.google-apps.folder";
    private const string DefaultFileFields = "id, name, mimeType, size, createdTime, modifiedTime, webViewLink, parents";
    private const string DefaultListFields = "nextPageToken, files(id, name, mimeType, size, createdTime, modifiedTime, webViewLink, parents)";

    private readonly GoogleDriveOptions _options;
    private readonly ILogger<GoogleDriveService> _logger;

    public GoogleDriveService(IOptions<GoogleDriveOptions> options, ILogger<GoogleDriveService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<DriveFileDto> CreateFolderAsync(
        string userAccessToken,
        string folderName,
        string? parentFolderId = null,
        CancellationToken cancellationToken = default)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Google Drive Create Folder");
        activity?.SetTag("FolderName", folderName);

        using var service = CreateDriveClient(userAccessToken);

        var fileMetadata = new GoogleDriveFile
        {
            Name = folderName,
            MimeType = FolderMimeType,
        };

        if (parentFolderId is not null)
        {
            fileMetadata.Parents = [parentFolderId];
        }

        var request = service.Files.Create(fileMetadata);
        request.Fields = DefaultFileFields;

        _logger.LogInformation("Creating Google Drive folder '{FolderName}' in parent '{ParentFolderId}'", folderName, parentFolderId);

        var folder = await request.ExecuteAsync(cancellationToken);

        return MapToDto(folder);
    }

    public async Task<DriveFileDto> UploadFileAsync(
        string userAccessToken,
        string fileName,
        Stream contentStream,
        string contentType,
        string? parentFolderId = null,
        CancellationToken cancellationToken = default)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Google Drive Upload File");
        activity?.SetTag("FileName", fileName);

        using var service = CreateDriveClient(userAccessToken);

        var fileMetadata = new GoogleDriveFile
        {
            Name = fileName,
        };

        if (parentFolderId is not null)
        {
            fileMetadata.Parents = [parentFolderId];
        }

        var request = service.Files.Create(fileMetadata, contentStream, contentType);
        request.Fields = DefaultFileFields;

        _logger.LogInformation("Uploading file '{FileName}' ({ContentType}) to Google Drive folder '{ParentFolderId}'",
            fileName, contentType, parentFolderId);

        var progress = await request.UploadAsync(cancellationToken);

        if (progress.Status == UploadStatus.Failed)
        {
            _logger.LogError(progress.Exception, "Failed to upload file '{FileName}' to Google Drive", fileName);
            throw progress.Exception;
        }

        return MapToDto(request.ResponseBody);
    }

    public async Task<Stream> DownloadFileAsync(
        string userAccessToken,
        string fileId,
        CancellationToken cancellationToken = default)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Google Drive Download File");
        activity?.SetTag("FileId", fileId);

        using var service = CreateDriveClient(userAccessToken);

        _logger.LogInformation("Downloading file '{FileId}' from Google Drive", fileId);

        var request = service.Files.Get(fileId);
        var memoryStream = new MemoryStream();
        await request.DownloadAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        return memoryStream;
    }

    public async Task<DriveFileListDto> ListFilesAsync(
        string userAccessToken,
        string? folderId = null,
        string? query = null,
        int maxResults = 100,
        string? pageToken = null,
        CancellationToken cancellationToken = default)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Google Drive List Files");

        using var service = CreateDriveClient(userAccessToken);

        var request = service.Files.List();
        request.PageSize = maxResults;
        request.Fields = DefaultListFields;

        var queryParts = new List<string> { "trashed = false" };

        if (folderId is not null)
        {
            queryParts.Add($"'{folderId}' in parents");
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            queryParts.Add(query);
        }

        request.Q = string.Join(" and ", queryParts);

        if (!string.IsNullOrWhiteSpace(pageToken))
        {
            request.PageToken = pageToken;
        }

        _logger.LogInformation("Listing Google Drive files with query '{Query}', maxResults {MaxResults}", request.Q, maxResults);

        var response = await request.ExecuteAsync(cancellationToken);

        var files = response.Files is not null
            ? response.Files.Select(MapToDto).ToList().AsReadOnly()
            : (IReadOnlyList<DriveFileDto>)[];

        return new DriveFileListDto(files, response.NextPageToken);
    }

    public async Task DeleteFileAsync(
        string userAccessToken,
        string fileId,
        CancellationToken cancellationToken = default)
    {
        using Activity? activity = DiagnosticsConfig.ActivitySource.StartActivity("Google Drive Delete File");
        activity?.SetTag("FileId", fileId);

        using var service = CreateDriveClient(userAccessToken);

        _logger.LogInformation("Deleting file '{FileId}' from Google Drive", fileId);

        await service.Files.Delete(fileId).ExecuteAsync(cancellationToken);
    }

    internal DriveService CreateDriveClient(string userAccessToken)
    {
        var credential = GoogleCredential.FromAccessToken(userAccessToken);

        return new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = _options.ApplicationName,
        });
    }

    internal static DriveFileDto MapToDto(GoogleDriveFile file)
    {
        return new DriveFileDto(
            Id: file.Id,
            Name: file.Name,
            MimeType: file.MimeType,
            Size: file.Size,
            CreatedTime: file.CreatedTimeDateTimeOffset,
            ModifiedTime: file.ModifiedTimeDateTimeOffset,
            WebViewLink: file.WebViewLink,
            Parents: file.Parents is not null
                ? file.Parents.ToList().AsReadOnly()
                : []
        );
    }
}
