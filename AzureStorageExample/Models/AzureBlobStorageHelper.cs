using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
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
            _storageAccount = CloudStorageAccount.Parse( CloudConfigurationManager.GetSetting(nameOfConnectionStringInAppSettings));

            // Create a CloudFileClient object for credentialed access to Azure Files.
            _blobClient = _storageAccount.CreateCloudBlobClient();

            // Get a reference to the file share we created previously.            
            var containerName = CloudConfigurationManager.GetSetting(nameOfStorageAccountShareInAppSettings);
            _blobContainer = _blobClient.GetContainerReference(containerName);

            // Ensure that the share exists.
            if (_blobContainer.Exists() == false)
                throw new Exception($"The storage account blob container named '{containerName}' does NOT exist!");
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

        /// <summary>Delete a blob</summary>
        /// <param name="blobName">The blob name and any path that is needed (e.g., 'Images/Test.jpg' or if it's at the root 'Test.jpg')</param>
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
        public List<CloudBlockBlob> ListFiles(string prefix, bool useFlatBlobListing, int maximumNumberOfFiles = -1)
        {
            var result = new List<CloudBlockBlob>();
           
            foreach (IListBlobItem item in _blobContainer.ListBlobs(prefix, useFlatBlobListing: useFlatBlobListing))
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

        /// <summary>Returns a blob as a byte array.</summary>
        /// <param name="blobName">A path to a blob (e.g., 2018\05\12\1131 W. Warner Road.JPG)</param>
        /// <returns>A byte array that represents the file.</returns>
        public byte[] LoadBlobAsByteArray(string blobName)
        {
            using (var blobStream = LoadBlobAsStream(blobName))
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
        public Stream LoadBlobAsStream(string blobName)
        {
            CloudBlockBlob blockBlob = FindBlob(blobName);

            if (blockBlob.Exists())
                return blockBlob.OpenRead();

            throw new FileNotFoundException($"Could not find the blob named: {blobName}");
        }



        /// <summary>Uploads a stream to the named blob</summary>
        /// <param name="blobName">The blob name and any path that is needed (e.g., 'Images/Test.jpg' or if it's at the root 'Test.jpg')</param>
        /// <param name="fileStream">A filestream to upload.  You are responsible for disposing of the stream!</param>
        public void UploadBlob(string blobName, System.IO.Stream fileStream)
        {
            CloudBlockBlob blockBlob = FindBlob(blobName);
                 
            // MimeMapping.GetMimeMapping works fine with partial paths
            string mimeType = MimeMapping.GetMimeMapping(blobName);
            if (string.IsNullOrWhiteSpace(mimeType) == false)
            {
                blockBlob.Properties.ContentType = mimeType;
            }

            // Create or overwrite the "blockBlob" blob with contents from a local file.
            // Note:  The SDK will calculate the MD5 hash of this blob for you as long as your using
            //        non-block methods like UploadFromStream.  If we EVER use block upload methods 
            //        like PutBlockList, we will have to calculate it ourselves. See the following link
            //        for more info on Block uploads: https://stackoverflow.com/questions/44240655/content-md5-is-missing-azure-portal
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