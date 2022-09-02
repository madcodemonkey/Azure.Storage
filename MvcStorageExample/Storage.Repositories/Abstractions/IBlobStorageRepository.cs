using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Storage.Repositories;

public interface IBlobStorageRepository
{
    /// <summary>Creates a wrapper around the blob so that you can perform actions on it (doesn't do anything to the server).</summary>
    /// <param name="blobName">The blob name and any path that is needed (e.g., 'Images/Test.jpg' or if it's at the root 'Test.jpg')</param>
    /// <returns>A BlobClient that represents the file.</returns>
    BlobClient CreateClient(string blobName);

    /// <summary>Exists</summary>
    /// <param name="blobName">The blob name and any path that is needed (e.g., 'Images/Test.jpg' or if it's at the root 'Test.jpg')</param>
    Task<bool> ExistsAsync(string blobName);

    /// <summary>Delete a blob</summary>
    /// <param name="blobName">The blob name and any path that is needed (e.g., 'Images/Test.jpg' or if it's at the root 'Test.jpg')</param>
    Task DeleteAsync(string blobName);

    /// <summary>Downloads a blob as a stream.</summary>
    /// <param name="blobName">The name of the blob</param>
    /// <param name="checkIfExists">Do you want to check if it exists before attempting to download the blob?</param>
    /// <returns>A stream or null</returns>
    Task<Stream> DownloadAsync(string blobName, bool checkIfExists = true);

    /// <summary>Uploads a stream to the named blob</summary>
    /// <param name="blobName">The blob name and any path that is needed (e.g., 'Images/Test.jpg' or if it's at the root 'Test.jpg')</param>
    /// <param name="fileStream">A fileStream to upload.  You are responsible for disposing of the stream!</param>
    /// <param name="metadataList">An optional metadata list</param>
    Task<BlobClient> UploadAsync(string blobName, Stream fileStream, Dictionary<string, string>? metadataList = null);

    /// <summary>List files</summary>
    Task<List<BlobItem>> ListFilesAsync(string? prefix = null, BlobTraits traits = BlobTraits.None,
        BlobStates states = BlobStates.None, CancellationToken continuationToken = default);
}