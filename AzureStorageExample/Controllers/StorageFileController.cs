using Storage.Repositories;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using StorageExamples.Models;

namespace StorageExamples.Controllers
{
    public class StorageFileController : Controller
    {
        public async Task<ActionResult> Index()
        {
            var fileRepository = new FileStorageRepository(GetStorageSettings());

            var fileList = new List<FileListResult>();
            foreach (var cloudFile in await fileRepository.ListFilesAsync(null, true))
            {
                fileList.Add(new FileListResult {Name = cloudFile.Name, IsDirectory = cloudFile.IsDirectory });
            }

            return View(fileList);
        }

        [HttpPost]
        public async Task<ActionResult> DeleteFile(string fileName)
        {
            var fileRepository = new FileStorageRepository(GetStorageSettings()); 
            await fileRepository.DeleteAsync(fileName);

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> DownloadFile(string fileName)
        {
            var fileRepository = new FileStorageRepository(GetStorageSettings());

            Stream myFileStream =  await fileRepository.DownloadAsync(fileName);

            if (myFileStream == null)
                throw new FileNotFoundException($"Could not find the file named: {fileName}");


            // Determine the Mime Type
            string mimeType = MimeMapping.GetMimeMapping(fileName) ?? "application/octet-stream";

            // The stream will be destroyed by FileStreamResult.WriteFile deep within HttpResponseBase according to this post
            // https://stackoverflow.com/questions/3084366/how-do-i-dispose-my-filestream-when-implementing-a-file-download-in-asp-net
            return File(myFileStream, mimeType, fileName);
        }

        [HttpPost]
        public async Task<ActionResult> UploadFile()
        {
            var httpRequest = HttpContext.Request;

            if (httpRequest.Files.Count > 0)
            {
                const int fileIndex = 0;
                var fileRepository = new FileStorageRepository(GetStorageSettings());

                var fileName = FileUploadHelper.GetFileName(httpRequest, fileIndex);
                using (Stream fileStream = FileUploadHelper.GetInputStream(httpRequest, fileIndex))
                    await fileRepository.UploadAsync(fileStream, fileName);

                return RedirectToAction("Index");
            }

            throw new ArgumentException("File count is zero.");
        }

        private FileStorageSettings GetStorageSettings()
        {
            return new FileStorageSettings
            {
                ConnectionString = ConfigurationManager.AppSettings["StorageConnectionString"],
                ShareName = ConfigurationManager.AppSettings["ShareName"],
            };
        }
    }
}