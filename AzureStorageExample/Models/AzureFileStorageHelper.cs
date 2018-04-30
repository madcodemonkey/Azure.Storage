using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.File;

namespace StorageExamples.Models
{
    public class AzureFileStorageHelper
    {
        private CloudStorageAccount _storageAccount;
        private CloudFileClient _fileClient;
        private CloudFileShare _share;

        /// <summary>Cloud Storage Helper</summary>
        /// <param name="nameOfConnectionStringInAppSettings">The name of the key in your config file within the app settings section that has the connnection string</param>
        /// <param name="nameOfStorageAccountShareInAppSettings">The name of the key in your config file within the app settings section that has the name of your storage account share in Azure</param>
        public AzureFileStorageHelper(string nameOfConnectionStringInAppSettings, string nameOfStorageAccountShareInAppSettings)
        {
            // Parse the connection string and return a reference to the storage account.
            _storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting(nameOfConnectionStringInAppSettings));

            // Create a CloudFileClient object for credentialed access to Azure Files.
            _fileClient = _storageAccount.CreateCloudFileClient();

            // Get a reference to the file share we created previously.            
            _share = _fileClient.GetShareReference(
                CloudConfigurationManager.GetSetting(nameOfStorageAccountShareInAppSettings));

            // Ensure that the share exists.
            if (_share.Exists() == false)
                throw new Exception($"The storage account file share named '{nameOfStorageAccountShareInAppSettings}' does NOT exist!");


            // Get a reference to the root directory for the share.
            CloudRootDirectory = _share.GetRootDirectoryReference();
        }

        public CloudFileDirectory CloudRootDirectory { get; private set; }

        /// <summary>Deletes a file from a given directory.</summary>
        /// <param name="cloudDirectory">The directory that contains the file.</param>
        /// <param name="fileName">The name of the file to delete.</param>
        public void DeleteFile(CloudFileDirectory cloudDirectory, string fileName)
        {
            CloudFile fileToDelete = FindFile(cloudDirectory, fileName);
            if (fileToDelete.Exists())
                fileToDelete.Delete();
        }


        /// <summary>Finds a file from a given directory.</summary>
        /// <param name="cloudDirectory">The directory that might contain the file.</param>
        /// <param name="fileName">The name of the file to find.</param>
        public CloudFile FindFile(CloudFileDirectory cloudDirectory, string fileName)
        {
            if (cloudDirectory == null)
                throw new ArgumentNullException("You must specify a cloud directory object!");

            var file = new AzureFileStorageFileInfo(fileName);

            CloudFileDirectory currentDirectory = string.IsNullOrWhiteSpace(file.Directory) ?
                cloudDirectory : cloudDirectory.GetDirectoryReference(file.Directory);

            if (currentDirectory == null || currentDirectory.Exists() == false)
                throw new FileNotFoundException($"Could not find a sub-directory ({file.Directory}) in the file's path ({file.FileName})");

            return currentDirectory.GetFileReference(file.FileName);
        }

        /// <summary>List files</summary>
        /// <param name="cloudDirectory">The directory whos files you want to list.</param>
        public List<CloudFile> ListFiles(CloudFileDirectory cloudDirectory)
        {
            var result = new List<CloudFile>();

            // To get directories only: cloudDirectory.ListFilesAndDirectories().OfType<CloudFileDirectory>()
            // To get files only: cloudDirectory.ListFilesAndDirectories().OfType<CloudFile>()
            // Otherwise, you must case the IlistFileItem to one or the other to determine if you have found a directory or file.
            foreach (CloudFile item in cloudDirectory.ListFilesAndDirectories().OfType<CloudFile>())
            {
                result.Add(item);
            }

            return result;
        }

        /// <summary>Uploads a stream into the directory specified.</summary>
        /// <param name="fileStream">A filestream to upload.  You are responsible for disposing of the stream!</param>
        public CloudFile UploadFile(CloudFileDirectory cloudDirectory, string fileName, System.IO.Stream fileStream)
        {
            string fileNameOnly = Path.GetFileName(fileName);

            // File the file exists, it will be overwritten; otherwise, it will be created after being uploaded.
            var cloudFile = cloudDirectory.GetFileReference(fileNameOnly);

            cloudFile.UploadFromStream(fileStream);

            return cloudFile;
        }

    }
}