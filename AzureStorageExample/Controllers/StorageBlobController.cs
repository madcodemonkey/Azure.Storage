using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Microsoft.WindowsAzure.Storage.Blob;
using StorageExamples.Models;

namespace StorageExamples.Controllers
{
    public class StorageBlobController : Controller
    {
        // GET: StorageBlob
        public ActionResult Index()
        {
            var fileList = new List<string>();

            var helper = new AzureBlobStorageHelper("StorageConnectionString", "BlobContainerName");

            foreach(var cloudBlockBlob in helper.ListFiles(true, 15))
            {
                fileList.Add(cloudBlockBlob.Name);
            }

            return View(fileList);
        }

        public ActionResult DownloadBlob(string blobName)
        {
            return DownloadAzureBlob(blobName);
        }

        private FileResult DownloadAzureBlob(string blobName)
        {
            var helper = new AzureBlobStorageHelper("StorageConnectionString", "BlobContainerName");

            CloudBlockBlob myCloudBlob = helper.FindBlob(blobName);

            if (myCloudBlob.Exists() == false)
                throw new FileNotFoundException($"Could not find the blob named: {blobName}");

            // Get a stream to the blob
            var myStream = myCloudBlob.OpenRead();

            // Determine the Mime Type
            string mimeType = MimeMapping.GetMimeMapping(blobName) ?? "application/octet-stream";

            // The stream will be destoyed by FileSreamResult.WriteFile deep within HttpResponseBase according to this post
            // https://stackoverflow.com/questions/3084366/how-do-i-dispose-my-filestream-when-implementing-a-file-download-in-asp-net
            return File(myStream, mimeType, myCloudBlob.Name);
        }

    }
}