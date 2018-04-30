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
        private bool _initialized = false;
        private CloudStorageAccount _storageAccount;
        private CloudFileClient _fileClient;
        private CloudFileShare _share;
        private CloudFileDirectory _cloudRootDirectory;
        private readonly string _nameOfConnectionStringInAppSettings;
        private readonly string _nameOfStorageAccountShareInAppSettings;

        /// <summary>Cloud Storage Helper</summary>
        /// <param name="nameOfConnectionStringInAppSettings">The name of the key in your config file within the app settings section that has the connnection string</param>
        /// <param name="nameOfStorageAccountShareInAppSettings">The name of the key in your config file within the app settings section that has the name of your storage account share in Azure</param>
        public AzureFileStorageHelper(string nameOfConnectionStringInAppSettings, string nameOfStorageAccountShareInAppSettings)
        {
            _nameOfConnectionStringInAppSettings = nameOfConnectionStringInAppSettings;
            _nameOfStorageAccountShareInAppSettings = nameOfStorageAccountShareInAppSettings;
        }

        public CloudFileDirectory CloudRootDirectory {
            get
            {
                if (_initialized == false)
                    Initialize();
                return _cloudRootDirectory;
            }
            set
            {
                _cloudRootDirectory = value;
            }
        }

        public void Initialize()
        {
            // Parse the connection string and return a reference to the storage account.
            _storageAccount = CloudStorageAccount.Parse(
                CloudConfigurationManager.GetSetting(_nameOfConnectionStringInAppSettings));

            // Create a CloudFileClient object for credentialed access to Azure Files.
            _fileClient = _storageAccount.CreateCloudFileClient();

            // Get a reference to the file share we created previously.            
            _share = _fileClient.GetShareReference(
                CloudConfigurationManager.GetSetting(_nameOfStorageAccountShareInAppSettings));

            // Ensure that the share exists.
            if (_share.Exists() == false)
                throw new Exception($"The storage account file share named '{_nameOfStorageAccountShareInAppSettings}' does NOT exist!");


            // Get a reference to the root directory for the share.
            _cloudRootDirectory = _share.GetRootDirectoryReference();

            _initialized = true;
        }

        public CloudFile FindFile(CloudFileDirectory cloudDirectory, string fileName)
        {
            if (cloudDirectory == null)
                throw new ArgumentNullException("You must specify a cloud directory object!");

            if (_initialized == false)
                Initialize();

            var file = new AzureFileStorageFileInfo(fileName);

            CloudFileDirectory currentDirectory = string.IsNullOrWhiteSpace(file.Directory) ?
                cloudDirectory : cloudDirectory.GetDirectoryReference(file.Directory);

            if (currentDirectory == null || currentDirectory.Exists() == false)
                throw new FileNotFoundException($"Could not find a sub-directory ({file.Directory}) in the file's path ({file.FileName})");

            return currentDirectory.GetFileReference(file.FileName);
        }

        /// <summary>List files</summary>
        public List<CloudFile> ListFiles(CloudFileDirectory cloudDirectory)
        {
            if (_initialized == false)
                Initialize();

            var result = new List<CloudFile>();



            foreach (CloudFile item in cloudDirectory.ListFilesAndDirectories().OfType<CloudFile>())
            {
                result.Add(item);
            }

            return result;
        }
    }
}