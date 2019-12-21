using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace StorageExamples.Models
{
    public class AzureBlobStorageHelper
    {
        private CloudStorageAccount _storageAccount;
        private CloudBlobClient _blobClient;
        private CloudBlobContainer _blobContainer;
        
        /// <summary>Cloud Storage Helper</summary>
        /// <param name="nameOfConnectionStringInAppSettings">The name of the key in your config file within the app settings section that has the connnection string</param>
        /// <param name="nameOfStorageAccountShareInAppSettings">The name of the key in your config file within the app settings section that has the name of your storage account share in Azure</param>
        public AzureBlobStorageHelper(string nameOfConnectionStringInAppSettings, string nameOfStorageAccountShareInAppSettings)
        {
            // Parse the connection string and return a reference to the storage account.            
            string connectionString = ConfigurationManager.AppSettings[nameOfConnectionStringInAppSettings];
            _storageAccount = CloudStorageAccount.Parse(connectionString);

            // Create a CloudFileClient object for credentialed access to Azure Files.
            _blobClient = _storageAccount.CreateCloudBlobClient();

            // Get a reference to the file share we created previously.            
            var containerName = ConfigurationManager.AppSettings[nameOfStorageAccountShareInAppSettings];
            _blobContainer = _blobClient.GetContainerReference(containerName);
         }

        /// <summary>Find a blob</summary>
        /// <param name="blobName">The blob name and any path that is needed (e.g., 'Images/Test.jpg' or if it's at the root 'Test.jpg')</param>
        /// <returns>A CloudBlockBlob that represents the file.</returns>
        public CloudBlockBlob FindBlob(string blobName)
        {
            string blobWithoutStartingSlashes = RemoveSlashes(blobName);

            if (string.IsNullOrWhiteSpace(blobWithoutStartingSlashes))
                throw new ArgumentException("Please specify a blob name!");

            return _blobContainer.GetBlockBlobReference(blobName);
        }

        /// <summary>Exists</summary>
        /// <param name="blobName">The blob name and any path that is needed (e.g., 'Images/Test.jpg' or if it's at the root 'Test.jpg')</param>
        public async Task<bool> ExistsAsync(string blobName)
        {
            CloudBlockBlob blockBlob = FindBlob(blobName);

            return await blockBlob.ExistsAsync();
        }


        /// <summary>Delete a blob</summary>
        /// <param name="blobName">The blob name and any path that is needed (e.g., 'Images/Test.jpg' or if it's at the root 'Test.jpg')</param>
        public async Task DeleteBlobAsync(string blobName)
        {
            CloudBlockBlob blockBlob = FindBlob(blobName);

            // Delete the blob if it exists.  If you don't check and it doesn't exist, you will get a 404 error.
            if (await blockBlob.ExistsAsync())
              await blockBlob.DeleteAsync();
        }

        /// <summary>Gets a List of files and will continue loading them till the continuation token comes back null (no more result)
        /// It could blow your stack if too many files are returned.</summary>
        /// <param name="useFlatBlobListing">Indicates if you want files listed in root (false) or all files</param>
        /// <param name="maximumNumberOfFiles">Maximium number of files to retrieve (-1 indicates no maximum)</param>
        /// <returns></returns>
        public async Task<List<CloudBlockBlob>> ListFilesIgnoreMemoryConcernsAsync(string prefix, bool useFlatBlobListing, int? maximumNumberOfFiles = null)
        {
            var result = new List<CloudBlockBlob>();
            AzureBlobListResult queryResult = null;

            do
            {
                var query = new AzureBlogQuery()
                {
                    Prefix = prefix,
                    MaximumNumberOfFiles = maximumNumberOfFiles,
                    UseFlatBlobListing = useFlatBlobListing,
                    ContinuationToken = queryResult != null ? queryResult.ContinuationToken : null

                };

                queryResult = await ListFilesAsync(query);
                foreach (var cloudBlockBlob in queryResult.Files)
                {
                    result.Add(cloudBlockBlob);
                }
            }
            while (queryResult.MoreFiles);

            return result;
        }



        /// <summary>List files using blob result so that you have a continuation token if needed.</summary>
        /// <param name="useFlatBlobListing">Indicates if you want files listed in root (false) or all files</param>
        /// <param name="maximumNumberOfFiles">Maximium number of files to retrieve (-1 indicates no maximum)</param>
        /// <returns></returns>
        public async Task<AzureBlobListResult> ListFilesAsync(AzureBlogQuery continueData)
        {
            return await ListFilesAsync(continueData.Prefix, continueData.UseFlatBlobListing, continueData.WhatToRetrieve,
                continueData.ContinuationToken, continueData.MaximumNumberOfFiles);
        }

        /// <summary>List files, but still gives you back a result with a continuation token if needed.</summary>
        /// <param name="prefix">Any search prefix that you want to add to filter results.</param>
        /// <param name="useFlatBlobListing">Indicates if you want files listed in root (false) or all files</param>
        /// <param name="maximumNumberOfFiles">Maximium number of files to retrieve (null indicates no maximum)</param>
        /// <returns></returns>
        public async Task<AzureBlobListResult> ListFilesAsync(string prefix, bool useFlatBlobListing, int? maximumNumberOfFiles = null)
        {
            return await ListFilesAsync(prefix, useFlatBlobListing, BlobListingDetails.All, null, maximumNumberOfFiles);
        }

        /// <summary>Returns a blob as a byte array.</summary>
        /// <param name="blobName">A path to a blob (e.g., 2018\05\12\1131 W. Warner Road.JPG)</param>
        /// <returns>A byte array that represents the file.</returns>
        public async Task<byte[]> LoadBlobAsByteArrayAsync(string blobName)
        {
            using (var blobStream = await LoadBlobAsStreamAsync(blobName))
            using (var ms = new MemoryStream())
            {
                blobStream.CopyTo(ms);
                ms.Seek(0, SeekOrigin.Begin);
                return ms.ToArray();
            }
        }

        /// <summary>Returns a blob stream to a file given a path to the file.</summary>
        /// <param name="blobName">A path to a blob (e.g., 2018\05\12\1131 W. Warner Road.JPG)</param>
        /// <returns>A stream that you are responsible for disposing.</returns>
        public async Task<Stream> LoadBlobAsStreamAsync(string blobName)
        {
            CloudBlockBlob blockBlob = FindBlob(blobName);

            if (await blockBlob.ExistsAsync())
                return await blockBlob.OpenReadAsync();

            throw new FileNotFoundException($"Could not find the blob named: {blobName}");
        }



        /// <summary>Uploads a stream to the named blob</summary>
        /// <param name="blobName">The blob name and any path that is needed (e.g., 'Images/Test.jpg' or if it's at the root 'Test.jpg')</param>
        /// <param name="fileStream">A filestream to upload.  You are responsible for disposing of the stream!</param>
        /// <param name="metadataList">An optional metadata list</param>
        public async Task UploadBlobAsync(string blobName, System.IO.Stream fileStream, Dictionary<string, string> metadataList)
        {
            CloudBlockBlob blockBlob = FindBlob(blobName);

            // MimeMapping.GetMimeMapping works fine with partial paths
            string mimeType = FindMimeType(blobName);
            if (string.IsNullOrWhiteSpace(mimeType) == false)
            {
                blockBlob.Properties.ContentType = mimeType;
            }

            if (metadataList != null && metadataList.Count > 0)
            {
                foreach(KeyValuePair<string, string> item in metadataList)
                {
                    blockBlob.Metadata.Add(item.Key, item.Value);
                }
            }

            
            // Create or overwrite the "blockBlob" blob with contents from a local file.
            // Note:  The SDK will calculate the MD5 hash of this blob for you as long as your using
            //        non-block methods like UploadFromStream.  If we EVER use block upload methods 
            //        like PutBlockList, we will have to calculate it ourselves. See the following link
            //        for more info on Block uploads: https://stackoverflow.com/questions/44240655/content-md5-is-missing-azure-portal
            await blockBlob.UploadFromStreamAsync(fileStream);
        }

        public string FindMimeType(string fileName)
        {
            return MimeMapping.GetMimeMapping(fileName);
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


        /// <summary>List files</summary>
        /// <param name="useFlatBlobListing">Indicates if you want files listed in root (false) or all files</param>
        /// <param name="maximumNumberOfFiles">Maximium number of files to retrieve (-1 indicates no maximum)</param>
        /// <returns></returns>
        private async Task<AzureBlobListResult> ListFilesAsync(string prefix, bool useFlatBlobListing,
            BlobListingDetails whatToRetrieve, BlobContinuationToken continuationToken, int? maximumNumberOfFiles)
        {
            var result = new AzureBlobListResult()
            {
                ContinuationToken = continuationToken,
                Prefix = prefix,
                UseFlatBlobListing = useFlatBlobListing,
                MaximumNumberOfFiles = maximumNumberOfFiles
            };

            var listOfItems = await _blobContainer.ListBlobsSegmentedAsync(prefix, useFlatBlobListing, 
                whatToRetrieve, maximumNumberOfFiles, result.ContinuationToken, null, null);

            result.ContinuationToken = listOfItems.ContinuationToken;

            foreach (IListBlobItem item in listOfItems.Results)
            {
                if (maximumNumberOfFiles > -1 && result.Files.Count >= maximumNumberOfFiles)
                    break;

                if (item.GetType() == typeof(CloudBlockBlob))
                {
                    result.Files.Add((CloudBlockBlob)item);
                }
                //else if (item.GetType() == typeof(CloudPageBlob))
                //{
                //    CloudPageBlob pageBlob = (CloudPageBlob)item;
                //}
                //else if (item.GetType() == typeof(CloudBlobDirectory))
                //{
                //    CloudBlobDirectory directory = (CloudBlobDirectory)item;
                //}
            }

            return result;
        }

    }

    public class AzureBlogQuery
    {
        public BlobListingDetails WhatToRetrieve { get; set; } = BlobListingDetails.All;
        public BlobContinuationToken ContinuationToken { get; set; }
        public bool UseFlatBlobListing { get; set; }
        public string Prefix { get; set; }
        public int? MaximumNumberOfFiles { get; set; } 
    }

    public class AzureBlobListResult : AzureBlogQuery
    {
        public AzureBlobListResult()
        {
            Files = new List<CloudBlockBlob>();
        }
   
        public bool MoreFiles => ContinuationToken != null;

        public List<CloudBlockBlob> Files { get; set; }

  

    }
}

