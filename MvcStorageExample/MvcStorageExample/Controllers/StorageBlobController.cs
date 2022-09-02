using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using MvcStorageExample.Utility;
using Storage.Repositories;

namespace MvcStorageExample.Controllers;

public class StorageBlobController : Controller
{
    private readonly IBlobStorageRepository _blobStorageRepository;
    private readonly IFileHelperService _fileHelperService;

    public StorageBlobController(IBlobStorageRepository blobStorageRepository, IFileHelperService fileHelperService)
    {
        _blobStorageRepository = blobStorageRepository;
        _fileHelperService = fileHelperService;
    }

    public async Task<ActionResult> Index()
    {
        var fileList = new List<string>();


        var result = await _blobStorageRepository.ListFilesAsync();

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
        await _blobStorageRepository.DeleteAsync(blobName);

        return RedirectToAction("Index");
    }

    public async Task<ZipResult> ZipBlob(string blobName)
    {
        using (Ionic.Zip.ZipFile theZipFile = new Ionic.Zip.ZipFile())
        {

            List<BlobItem> blobList = await _blobStorageRepository.ListFilesAsync(blobName);
            foreach (BlobItem myCloudBlob in blobList)
            {
                BlobClient myBlobClient = _blobStorageRepository.CreateClient(myCloudBlob.Name);
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

        if (httpRequest.Form.Files.Count > 0)
        {
            const int fileIndex = 0;

            var directory = _fileHelperService.GetFormValue<string>(httpRequest, "directory", string.Empty);
            var fileName = string.IsNullOrWhiteSpace(directory) ?
                _fileHelperService.GetFileName(httpRequest, fileIndex) :
                Path.Combine(directory, _fileHelperService.GetFileName(httpRequest, fileIndex));

            using (Stream fileStream = _fileHelperService.GetInputStream(httpRequest, fileIndex))
                await _blobStorageRepository.UploadAsync(fileName, fileStream);

            return RedirectToAction("Index");
        }

        throw new ArgumentException("File count is zero.");
    }


    private async Task<FileResult> DownloadAzureBlob(string blobName)
    {
        var dataStream = await _blobStorageRepository.DownloadAsync(blobName);

        if (dataStream == null)
            throw new FileNotFoundException($"Could not find the blob named: {blobName}");
        
        string mimeType =_fileHelperService.DetermineMimeTypes(blobName);

        // The stream will be destoyed by FileSreamResult.WriteFile deep within HttpResponseBase according to this post
        // https://stackoverflow.com/questions/3084366/how-do-i-dispose-my-filestream-when-implementing-a-file-download-in-asp-net
        return File(dataStream, mimeType, blobName);
    }



}