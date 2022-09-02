using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Storage.Repositories
{
    public class BlobStorageRepository
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly BlobContainerClient _blobContainerClient;
        
        /// <summary>Constructor</summary>>
        public BlobStorageRepository(BlobStorageSettings settings)
        {
            _blobServiceClient = new BlobServiceClient(settings.ConnectionString);
            _blobContainerClient = _blobServiceClient.GetBlobContainerClient(settings.ContainerName);
        }

        /// <summary>Creates a wrapper around the blob so that you can perform actions on it (doesn't do anything to the server).</summary>
        /// <param name="blobName">The blob name and any path that is needed (e.g., 'Images/Test.jpg' or if it's at the root 'Test.jpg')</param>
        /// <returns>A BlobClient that represents the file.</returns>
        public BlobClient CreateClient(string blobName)
        {
            string blobWithoutStartingSlashes = RemoveSlashes(blobName);

            if (string.IsNullOrWhiteSpace(blobWithoutStartingSlashes))
                throw new ArgumentException("Please specify a blob name!");

            return _blobContainerClient.GetBlobClient(blobName);
        }

        /// <summary>Exists</summary>
        /// <param name="blobName">The blob name and any path that is needed (e.g., 'Images/Test.jpg' or if it's at the root 'Test.jpg')</param>
        public async Task<bool> ExistsAsync(string blobName)
        {
            var blockBlob = CreateClient(blobName);

            return await blockBlob.ExistsAsync();
        }


        /// <summary>Delete a blob</summary>
        /// <param name="blobName">The blob name and any path that is needed (e.g., 'Images/Test.jpg' or if it's at the root 'Test.jpg')</param>
        public async Task DeleteAsync(string blobName)
        {
            var blockBlob = CreateClient(blobName);

            // Delete the blob if it exists.  If you don't check and it doesn't exist, you will get a 404 error.
            if (await blockBlob.ExistsAsync())
                await blockBlob.DeleteAsync();
        }

        /// <summary>Downloads a blob as a stream.</summary>
        /// <param name="blobName">The name of the blob</param>
        /// <param name="checkIfExists">Do you want to check if it exists before attempting to download the blob?</param>
        /// <returns>A stream or null</returns>
        public async Task<Stream> DownloadAsync(string blobName, bool checkIfExists = true)
        {
            var blobClient = CreateClient(blobName);
            if (checkIfExists && await blobClient.ExistsAsync() == false) return null;

            BlobDownloadInfo download = await blobClient.DownloadAsync();

            return download.Content;
        }

        /// <summary>Uploads a stream to the named blob</summary>
        /// <param name="blobName">The blob name and any path that is needed (e.g., 'Images/Test.jpg' or if it's at the root 'Test.jpg')</param>
        /// <param name="fileStream">A fileStream to upload.  You are responsible for disposing of the stream!</param>
        /// <param name="metadataList">An optional metadata list</param>
        public async Task<BlobClient> UploadAsync(string blobName, Stream fileStream, Dictionary<string, string> metadataList = null)
        {
            BlobClient blobClient = CreateClient(blobName);

            
            if (metadataList != null && metadataList.Count > 0)
            {
                await blobClient.UploadAsync(content: fileStream, metadata: metadataList);
            }
            else
            {
                await blobClient.UploadAsync(fileStream);
            }

            return blobClient;
        }

        /// <summary>List files</summary>
        public async Task<List<BlobItem>> ListFilesAsync(string prefix = null, BlobTraits traits = BlobTraits.None,
            BlobStates states = BlobStates.None, CancellationToken continuationToken = default)
        {
            var result = new List<BlobItem>();

            // For >= C# 8.0 use:
            //await foreach (BlobItem blobItem in _blobContainerClient.GetBlobsAsync(traits, states, prefix, continuationToken))
            //{
            //    result.Add(blobItem);
            //}

            // For < C# 8.0 use:
            AsyncPageable<BlobItem> blobPages = _blobContainerClient.GetBlobsAsync(traits, states, prefix, continuationToken);
            IAsyncEnumerator<BlobItem> enumerator = blobPages.GetAsyncEnumerator();
            try
            {
                while (await enumerator.MoveNextAsync())
                {
                    result.Add(enumerator.Current);
                }
            }
            finally
            {
                await enumerator.DisposeAsync();
            }


            return result;
        }

        private static string RemoveSlashes(string blobName)
        {
            string blobWithStartingSlashes = blobName;

            // Remove slash
            while (string.IsNullOrWhiteSpace(blobWithStartingSlashes) == false && blobWithStartingSlashes.StartsWith(@"/"))
            {
                blobWithStartingSlashes = blobWithStartingSlashes.Substring(1);
            }

            // Remove backslash
            while (string.IsNullOrWhiteSpace(blobWithStartingSlashes) == false && blobWithStartingSlashes.StartsWith(@"\"))
            {
                blobWithStartingSlashes = blobWithStartingSlashes.Substring(1);
            }

            return blobWithStartingSlashes;
        }
    }
}