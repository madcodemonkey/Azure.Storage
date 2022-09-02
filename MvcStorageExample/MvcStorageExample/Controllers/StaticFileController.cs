using Microsoft.AspNetCore.Mvc;
using MvcStorageExample.Utility;

namespace MvcStorageExample.Controllers;

public class StaticFileController : Controller
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly IFileHelperService _fileHelperService;

    public StaticFileController(IWebHostEnvironment webHostEnvironment, IFileHelperService fileHelperService)
    {
        _webHostEnvironment = webHostEnvironment;
        _fileHelperService = fileHelperService;
    }

    public IActionResult Index()
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
        // _webHostEnvironment.WebRootPath: https://stackoverflow.com/a/55934673/97803
        string contentRootPath = Path.Combine(_webHostEnvironment.WebRootPath, "images");

        return DownloadFile(contentRootPath, fileName);
    }

    public FileResult DownloadFile(string fileDirectory, string fileName)
    {
        // Get file info and create a stream 
        // PhysicalFileProvider requires this using statement: using Microsoft.Extensions.FileProviders; 
        string fileNameWithPath = Path.Combine(fileDirectory, fileName);
        var readStream = System.IO.File.Open(fileNameWithPath, FileMode.Open);
        
        // Determine the Mime Type (https://stackoverflow.com/a/35880687/97803)
        string mimeType = _fileHelperService.DetermineMimeTypes(fileName);

        // The stream will be destoyed by FileSreamResult.WriteFile deep within HttpResponseBase according to this post
        // https://stackoverflow.com/questions/3084366/how-do-i-dispose-my-filestream-when-implementing-a-file-download-in-asp-net
        return File(readStream, mimeType, fileName);
    }
}