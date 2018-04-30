using System;
using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using Microsoft.WindowsAzure.Storage.File;
using StorageExamples.Models;

namespace StorageExamples.Controllers
{
    public class StorageFileController : Controller
    {
        public ActionResult Index()
        {
            var helper = new AzureFileStorageHelper("StorageConnectionString", "ShareName");

            var fileList = new List<string>();
            foreach (var cloudFile in helper.ListFiles(helper.CloudRootDirectory))
            {
                fileList.Add(cloudFile.Name);
            }            

            return View(fileList);
        }

        [HttpPost]
        public ActionResult DeleteFile(string fileName)
        {
            var helper = new AzureFileStorageHelper("StorageConnectionString", "ShareName");
            helper.DeleteFile(helper.CloudRootDirectory, fileName);

            return RedirectToAction("Index");
        }

        public ActionResult DownloadFile(string fileName)
        {
            return DownloadAzureFile(fileName);
        }

        private FileResult DownloadAzureFile(string fileName)
        {
            var helper = new AzureFileStorageHelper("StorageConnectionString", "ShareName");

            CloudFile myCloudFile = helper.FindFile(helper.CloudRootDirectory, fileName);

            if (myCloudFile.Exists() == false)
                throw new FileNotFoundException($"Could not find the file named: {fileName}");

            // Get a stream to the file
            var myStream = myCloudFile.OpenRead();

            // Determine the Mime Type
            string mimeType = MimeMapping.GetMimeMapping(fileName) ?? "application/octet-stream";

            // The stream will be destoyed by FileSreamResult.WriteFile deep within HttpResponseBase according to this post
            // https://stackoverflow.com/questions/3084366/how-do-i-dispose-my-filestream-when-implementing-a-file-download-in-asp-net
            return File(myStream, mimeType, myCloudFile.Name);
        }

        [HttpPost]
        public ActionResult UploadFile()
        {
            var httpRequest = HttpContext.Request;

            if (httpRequest.Files.Count > 0)
            {
                const int fileIndex = 0;
                var helper = new AzureFileStorageHelper("StorageConnectionString", "ShareName");

                var fileName = FileUploadHelper.GetFileName(httpRequest, fileIndex);
                using (Stream fileStream = FileUploadHelper.GetInputStream(httpRequest, fileIndex))
                    helper.UploadFile(helper.CloudRootDirectory, fileName, fileStream);
                
                return RedirectToAction("Index");
            }

            throw new ArgumentException("File count is zero.");
        }

    }
}