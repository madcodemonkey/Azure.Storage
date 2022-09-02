using Microsoft.AspNetCore.Mvc;
using MvcStorageExample.Utility;
using Storage.Models;
using Storage.Repositories;

namespace MvcStorageExample.Controllers;

public class StorageFileController : Controller
{
    private readonly IFileStorageRepository _fileStorageRepository;
    private readonly IFileHelperService _fileHelperService;

    public StorageFileController(IFileStorageRepository fileStorageRepository, IFileHelperService fileHelperService)
    {
        _fileStorageRepository = fileStorageRepository;
        _fileHelperService = fileHelperService;
    }

    public async Task<ActionResult> Index()
    {
        var fileList = new List<FileListResult>();
        foreach (var cloudFile in await _fileStorageRepository.ListFilesAsync(null, true))
        {
            fileList.Add(new FileListResult { Name = cloudFile.Name, IsDirectory = cloudFile.IsDirectory });
        }

        return View(fileList);
    }

    [HttpPost]
    public async Task<ActionResult> DeleteFile(string fileName)
    {
         await _fileStorageRepository.DeleteAsync(fileName);

        return RedirectToAction("Index");
    }

    public async Task<ActionResult> DownloadFile(string fileName)
    {
     
        Stream myFileStream = await _fileStorageRepository.DownloadAsync(fileName);

        if (myFileStream == null)
            throw new FileNotFoundException($"Could not find the file named: {fileName}");


        // Determine the Mime Type
        string mimeType = _fileHelperService.DetermineMimeTypes(fileName);

        // The stream will be destroyed by FileStreamResult.WriteFile deep within HttpResponseBase according to this post
        // https://stackoverflow.com/questions/3084366/how-do-i-dispose-my-filestream-when-implementing-a-file-download-in-asp-net
        return File(myFileStream, mimeType, fileName);
    }

    [HttpPost]
    public async Task<ActionResult> UploadFile()
    {
        var httpRequest = HttpContext.Request;

        if (httpRequest.Form.Files.Count > 0)
        {
            const int fileIndex = 0;
          
            var fileName = _fileHelperService.GetFileName(httpRequest, fileIndex);
            using (Stream fileStream = _fileHelperService.GetInputStream(httpRequest, fileIndex))
                await _fileStorageRepository.UploadAsync(fileStream, fileName);

            return RedirectToAction("Index");
        }

        throw new ArgumentException("File count is zero.");
    }
     
}