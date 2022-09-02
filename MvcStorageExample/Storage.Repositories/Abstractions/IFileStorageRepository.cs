using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;

namespace Storage.Repositories;

public interface IFileStorageRepository
{
    /// <summary>Creates a wrapper around the file so that you can perform actions on it (doesn't do anything to the server).</summary>
    /// <param name="fileName">The file name (the path will be extracted from the file name)</param>
    ShareFileClient CreateClient(string fileName);

    /// <summary>Creates a wrapper around the file so that you can perform actions on it (doesn't do anything to the server).</summary>
    /// <param name="directoryName">The directory of interest or empty/null if you want the root directory</param>
    /// <param name="fileName">The file name without a path</param>
    ShareFileClient CreateClient(string directoryName, string fileName);

    /// <summary>Delete a file</summary>
    /// <param name="fileName">The file name (the path will be extracted from the file name)</param>
    Task DeleteAsync(string fileName);

    /// <summary>Delete a file</summary>
    /// <param name="directoryName">The directory of interest or empty/null if you want the root directory</param>
    /// <param name="fileName">The file name without a path</param>
    Task DeleteAsync(string directoryName, string fileName);

    /// <summary>Returns a stream so you can download a file</summary>
    /// <param name="fileName">The file name (the path will be extracted from the file name)</param>
    Task<Stream> DownloadAsync(string fileName);

    /// <summary>Returns a stream so you can download a file</summary>
    /// <param name="directoryName">The directory of interest or empty/null if you want the root directory</param>
    /// <param name="fileName">The file name without a path</param>
    Task<Stream> DownloadAsync(string directoryName, string fileName);

    /// <summary>List files</summary>
    /// <param name="directoryName">The directory of interest or null if you want the root directory</param>
    /// <param name="returnDirectoriesAndFiles">If true, you will get both directories and file.  If false, you will get only files.</param>
    /// <param name="cancellationToken"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    Task<List<ShareFileItem>> ListFilesAsync(string directoryName,
        bool returnDirectoriesAndFiles, CancellationToken cancellationToken = default,
        ShareDirectoryGetFilesAndDirectoriesOptions options = null);

    /// <summary>Uploads a file</summary>
    /// <param name="fileStream">The file stream to upload.</param>
    /// <param name="fileName">The file name (the path will be extracted from the file name)</param>
    Task UploadAsync(Stream fileStream, string fileName);

    /// <summary>Uploads a file</summary>
    /// <param name="fileStream">The file stream to upload.</param>
    /// <param name="directoryName">The directory of interest or empty/null if you want the root directory</param>
    /// <param name="fileName">The file name without a path</param>
    Task UploadAsync(Stream fileStream, string directoryName, string fileName);
}