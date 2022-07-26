using Ionic.Zip;
using StorageExamples.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Storage.Repositories;

namespace StorageExamples.Controllers
{
    public class StorageBlobController : Controller
    {
        public async Task<ActionResult> Index()
        {
            var fileList = new List<string>();

            var blobRepository = new BlobStorageRepository(GetStorageSettings());

            var result = await blobRepository.ListFilesAsync();

            foreach (var blobItem in result)
            {
                fileList.Add(blobItem.Name);
            }

            return View(fileList);
        }

        public async Task<ActionResult> DownloadBlob(string blobName)
        {
            return await DownloadAzureBlob(blobName);
        }

        [HttpPost]
        public async Task<ActionResult> DeleteBlob(string blobName)
        {
            var blobRepository = new BlobStorageRepository(GetStorageSettings());
            await blobRepository.DeleteAsync(blobName);

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> ZipBlob(string blobName)
        {
            var blobRepository = new BlobStorageRepository(GetStorageSettings());
            
            var zipFileStream = new MemoryStream();


            using (ZipFile theZipFile = new ZipFile())
            {

                List<BlobItem> blobList =  await blobRepository.ListFilesAsync(blobName);
                foreach (BlobItem myCloudBlob in blobList)
                {
                    BlobClient myBlobClient = blobRepository.CreateClient(myCloudBlob.Name);
                    var myStream = myBlobClient.OpenRead();

                    // Remove first part of the path
                    if (myCloudBlob.Name.StartsWith("Images"))
                    {
                        string newName = myCloudBlob.Name.Substring(7);
                        theZipFile.AddEntry(newName, myStream);
                    }
                    else theZipFile.AddEntry(myCloudBlob.Name, myStream);

                }

                return new ZipResult(theZipFile, "Report.zip");
            }
        }


        [HttpPost]
        public async Task<ActionResult> UploadFile()
        {
            var httpRequest = HttpContext.Request;

            if (httpRequest.Files.Count > 0)
            {
                const int fileIndex = 0;
                var blobRepository = new BlobStorageRepository(GetStorageSettings());

                var directory = FileUploadHelper.GetFormValue<string>(httpRequest, "directory", string.Empty);
                var fileName = string.IsNullOrWhiteSpace(directory) ?
                    FileUploadHelper.GetFileName(httpRequest, fileIndex) :
                    Path.Combine(directory, FileUploadHelper.GetFileName(httpRequest, fileIndex));

                using (Stream fileStream = FileUploadHelper.GetInputStream(httpRequest, fileIndex))
                    await blobRepository.UploadAsync(fileName, fileStream);

                return RedirectToAction("Index");
            }

            throw new ArgumentException("File count is zero.");
        }


        private async Task<FileResult> DownloadAzureBlob(string blobName)
        {
            var blobRepository = new BlobStorageRepository(GetStorageSettings());

            var dataStream = await blobRepository.DownloadAsync(blobName);

            if (dataStream == null)
                throw new FileNotFoundException($"Could not find the blob named: {blobName}");
            
            // Determine the Mime Type
            string mimeType = MimeMapping.GetMimeMapping(blobName) ?? "application/octet-stream";

            // The stream will be destoyed by FileSreamResult.WriteFile deep within HttpResponseBase according to this post
            // https://stackoverflow.com/questions/3084366/how-do-i-dispose-my-filestream-when-implementing-a-file-download-in-asp-net
            return File(dataStream, mimeType, blobName);
        }

        private BlobStorageSettings GetStorageSettings()
        {
            return new BlobStorageSettings()
            {
                ConnectionString = ConfigurationManager.AppSettings["StorageConnectionString"],
                ContainerName = ConfigurationManager.AppSettings["BlobContainerName"],
            };
        }


    }
}