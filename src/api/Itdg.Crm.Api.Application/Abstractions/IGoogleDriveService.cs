namespace Itdg.Crm.Api.Application.Abstractions;

using Itdg.Crm.Api.Application.Dtos;

/// <summary>
/// Provides Google Drive API operations using per-user OAuth 2.0 tokens.
/// </summary>
public interface IGoogleDriveService
{
    /// <summary>
    /// Creates a folder in Google Drive.
    /// </summary>
    /// <param name="userAccessToken">The user's OAuth 2.0 access token.</param>
    /// <param name="folderName">The name of the folder to create.</param>
    /// <param name="parentFolderId">Optional parent folder ID. If null, creates in root.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created folder metadata.</returns>
    Task<DriveFileDto> CreateFolderAsync(
        string userAccessToken,
        string folderName,
        string? parentFolderId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Uploads a file to Google Drive.
    /// </summary>
    /// <param name="userAccessToken">The user's OAuth 2.0 access token.</param>
    /// <param name="fileName">The name of the file to upload.</param>
    /// <param name="contentStream">The file content stream.</param>
    /// <param name="contentType">The MIME type of the file.</param>
    /// <param name="parentFolderId">Optional parent folder ID. If null, uploads to root.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The uploaded file metadata.</returns>
    Task<DriveFileDto> UploadFileAsync(
        string userAccessToken,
        string fileName,
        Stream contentStream,
        string contentType,
        string? parentFolderId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a file from Google Drive.
    /// </summary>
    /// <param name="userAccessToken">The user's OAuth 2.0 access token.</param>
    /// <param name="fileId">The Google Drive file ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The file content stream.</returns>
    Task<Stream> DownloadFileAsync(
        string userAccessToken,
        string fileId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists files in a Google Drive folder.
    /// </summary>
    /// <param name="userAccessToken">The user's OAuth 2.0 access token.</param>
    /// <param name="folderId">Optional folder ID to list files from. If null, lists from root.</param>
    /// <param name="query">Optional Drive search query.</param>
    /// <param name="maxResults">Maximum number of files to return (default 100).</param>
    /// <param name="pageToken">Token for pagination of results.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A paginated list of Drive files.</returns>
    Task<DriveFileListDto> ListFilesAsync(
        string userAccessToken,
        string? folderId = null,
        string? query = null,
        int maxResults = 100,
        string? pageToken = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a file or folder from Google Drive.
    /// </summary>
    /// <param name="userAccessToken">The user's OAuth 2.0 access token.</param>
    /// <param name="fileId">The Google Drive file or folder ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task DeleteFileAsync(
        string userAccessToken,
        string fileId,
        CancellationToken cancellationToken = default);
}
