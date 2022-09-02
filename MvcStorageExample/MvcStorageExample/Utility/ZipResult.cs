using Ionic.Zip;
using Microsoft.AspNetCore.Mvc;

namespace MvcStorageExample.Utility;

public class ZipResult : ActionResult
{
    private readonly ZipFile _theZipFile;
    private readonly string _fileName;

    public ZipResult(ZipFile theZipFile, string fileName)
    {
        _theZipFile = theZipFile;
        _fileName = fileName;
    }

    public override void ExecuteResult(ActionContext context)
    {
        context.HttpContext.Response.Headers.Clear();
        context.HttpContext.Response.ContentType = "application/zip";
        context.HttpContext.Response.Headers.Add("content-disposition", $"attachment; filename={_fileName}");
        _theZipFile.Save(context.HttpContext.Response.Body);
    }
}