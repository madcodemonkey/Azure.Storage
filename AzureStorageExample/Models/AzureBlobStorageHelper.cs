using System;
using System.Collections.Generic;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace StorageExamples.Models
{
    public class AzureBlobStorageHelper
    {
        private bool _initialized = false;
        private CloudStorageAccount _storageAccount;
        private CloudBlobClient _blogClient;
        private CloudBlobContainer _blogContainer;
         private readonly string _nameOfConnectionStringInAppSettings;
        private readonly string _nameOfStorageAccountShareInAppSettings;

        /// <summary>Cloud Storage Helper</summary>
        /// <param name="nameOfConnectionStringInAppSettings">The name of the key in your config file within the app settings section that has the connnection string</param>
        /// <param name="nameOfStorageAccountShareInAppSettings">The name of the key in your config file within the app settings section that has the name of your storage account share in Azure</param>
        public AzureBlobStorageHelper(string nameOfConnectionStringInAppSettings, string nameOfStorageAccountShareInAppSettings)
        {
            _nameOfConnectionStringInAppSettings = nameOfConnectionStringInAppSettings;
            _nameOfStorageAccountShareInAppSettings = nameOfStorageAccountShareInAppSettings;
        }
           
        public void Initialize()
        {
            // Parse the connection string and return a reference to the storage account.
            _storageAccount = CloudStorageAccount.Parse( CloudConfigurationManager.GetSetting(_nameOfConnectionStringInAppSettings));

            // Create a CloudFileClient object for credentialed access to Azure Files.
            _blogClient = _storageAccount.CreateCloudBlobClient();

            // Get a reference to the file share we created previously.            
            var containerName = CloudConfigurationManager.GetSetting(_nameOfStorageAccountShareInAppSettings);
            _blogContainer = _blogClient.GetContainerReference(containerName);

            // Ensure that the share exists.
            if (_blogContainer.Exists() == false)
                throw new Exception($"The storage account blob container named '{containerName}' does NOT exist!");

            _initialized = true;
        }

        public CloudBlockBlob FindBlob(string blobName)
        {
            if (_initialized == false)
                Initialize();

            string blobWithoutStartingSlashes = RemoveSlashes(blobName);

            if (string.IsNullOrWhiteSpace(blobWithoutStartingSlashes))
                throw new ArgumentException("Please specify a blob name!");
            
            return _blogContainer.GetBlockBlobReference(blobName);
        }

        public void DeleteBlob(string blobName)
        {
            CloudBlockBlob blockBlob = FindBlob(blobName);

            // Delete the blob if it exists.  If you don't check and it doesn't exist, you will get a 404 error.
            if (blockBlob.Exists())
                blockBlob.Delete();
        }


        /// <summary>List files</summary>
        /// <param name="useFlatBlobListing">Indicates if you want files listed in root (false) or all files</param>
        /// <param name="maximumNumberOfFiles">Maximium number of files to retrieve (-1 indicates no maximum)</param>
        /// <returns></returns>
        public List<CloudBlockBlob> ListFiles(bool useFlatBlobListing, int maximumNumberOfFiles = -1)
        {
            if (_initialized == false)
                Initialize();

            var result = new List<CloudBlockBlob>();

            foreach (IListBlobItem item in _blogContainer.ListBlobs(null, useFlatBlobListing: useFlatBlobListing))
            {
                if (maximumNumberOfFiles > -1 && result.Count >= maximumNumberOfFiles)
                    break;

                if (item.GetType() == typeof(CloudBlockBlob))
                {
                    result.Add((CloudBlockBlob)item);
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

        /// <summary>Uploads a stream to the named blob</summary>
        /// <param name="blobName">Name of the blob with folder names in front (e.g., Images/MyPicture.jpg  or if in the root MyPicture.jpg)</param>
        /// <param name="fileStream">A filestream to upload.  You are responsible for disposing of the stream!</param>
        public void UploadBlob(string blobName, System.IO.Stream fileStream)
        {
            CloudBlockBlob blockBlob = FindBlob(blobName);

            // Create or overwrite the "myblob" blob with contents from a local file.
            blockBlob.UploadFromStream(fileStream);
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