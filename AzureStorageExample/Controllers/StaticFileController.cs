using System.Collections.Generic;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace StorageExamples.Controllers
{
    public class StaticFileController : Controller
    {
        // GET: StaticFile
        public ActionResult Index()
        {
            var fileNames = new List<string>() {
                "banner1.svg",
                "banner2.svg",
                "banner3.svg",
                "banner4.svg",
                "Test.jpg"
            };

            return View(fileNames);
        }

        public ActionResult DownloadStaticFile(string fileName)
        {
            return DownloadFile(Server.MapPath("~/Content/Images"), fileName);
        }

        public FileResult DownloadFile(string fileDirectory, string fileName)
        {

            // Get file info and create a stream 
            // PhysicalFileProvider requires this using statement: using Microsoft.Extensions.FileProviders; 
            string fileNameWithPath = Path.Combine(fileDirectory, fileName);
            var readStream = System.IO.File.Open(fileNameWithPath, FileMode.Open);

            // Determine the Mime Type
            string mimeType = MimeMapping.GetMimeMapping(fileName) ?? "application/octet-stream";

            // The stream will be destoyed by FileSreamResult.WriteFile deep within HttpResponseBase according to this post
            // https://stackoverflow.com/questions/3084366/how-do-i-dispose-my-filestream-when-implementing-a-file-download-in-asp-net
            return File(readStream, mimeType, fileName);
        }
    }
}