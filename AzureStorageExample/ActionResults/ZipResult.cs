using Ionic.Zip;
using System.Web.Mvc;

namespace StorageExamples
{
    public class ZipResult : ActionResult
    {
        private readonly ZipFile _theZipFile;
        private string _fileName;

        public ZipResult(ZipFile theZipFile, string fileName)
        {
            _theZipFile = theZipFile;
            _fileName = fileName;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.ClearContent();
            context.HttpContext.Response.ClearHeaders();
            context.HttpContext.Response.ContentType = "application/zip";
            context.HttpContext.Response.AppendHeader("content-disposition", $"attachment; filename={_fileName}");
            _theZipFile.Save(context.HttpContext.Response.OutputStream);
        }
    }

}