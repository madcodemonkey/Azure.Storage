﻿using Microsoft.AspNetCore.StaticFiles;

namespace MvcStorageExample.Utility;

public interface IFileHelperService
{
    /// <summary>Extracts the mimetype from a file name.  Returns 'application/octet-stream' if the type cannot be determined.</summary>
    /// <param name="fileName">The name of the file (must have an extension)</param>
    string DetermineMimeTypes(string fileName);

    /// <summary>Gets a form value sent with the uploaded file(s)</summary>
    /// <typeparam name="T">Type of the data that was sent</typeparam>
    /// <param name="httpRequest">Http Request that holds the file</param>
    /// <param name="name">Name of the form value</param>
    /// <param name="defaultIfNotFound">What you want to return if the form value isn't found.</param>
    /// <returns></returns>
    T GetFormValue<T>(HttpRequest httpRequest, string name, T defaultIfNotFound);

    /// <summary>Gets the file name at the given index</summary>
    /// <param name="httpRequest">Http Request that holds the file</param>
    /// <param name="zeroBasedFileIndex">Zero based index of the file you desire</param>
    string GetFileName(HttpRequest httpRequest, int zeroBasedFileIndex);

    /// <summary>Gives you a file stream for the incoming file.  Should you dispose it?
    /// It's not necessary according the the official guidance</summary>
    /// <param name="httpRequest">Http Request that holds the file</param>
    /// <param name="zeroBasedFileIndex">Zero based index of the file you desire</param>
    /// <remarks>
    /// Note 1: https://docs.microsoft.com/en-us/dotnet/api/system.web.httppostedfile?redirectedfrom=MSDN&view=netframework-4.8
    ///         States, "Server resources that are allocated to buffer the uploaded file will be destroyed when the request ends." 
    /// Note 2: Interesting read https://stackoverflow.com/questions/37139995/should-i-dispose-fileupload-postedfile-inputstream
    /// </remarks>
    Stream GetInputStream(HttpRequest httpRequest, int zeroBasedFileIndex);
}

/// <summary>Helps you deal with files being uploaded by users.</summary>
public class FileHelperService : IFileHelperService
{
    /// <summary>Extracts the mimetype from a file name.  Returns 'application/octet-stream' if the type cannot be determined.</summary>
    /// <param name="fileName">The name of the file (must have an extension)</param>
    public string DetermineMimeTypes(string fileName)
    {
        // Determine the Mime Type (https://stackoverflow.com/a/35880687/97803)
        if (new FileExtensionContentTypeProvider().TryGetContentType(fileName, out var mimeType) == false)
            mimeType = "application/octet-stream";
        return mimeType;
    }


    /// <summary>Gets a form value sent with the uploaded file(s)</summary>
    /// <typeparam name="T">Type of the data that was sent</typeparam>
    /// <param name="httpRequest">Http Request that holds the file</param>
    /// <param name="name">Name of the form value</param>
    /// <param name="defaultIfNotFound">What you want to return if the form value isn't found.</param>
    /// <returns></returns>
    public T GetFormValue<T>(HttpRequest httpRequest, string name, T defaultIfNotFound)
    {
        string[] strings = httpRequest.Form[name];
        if (strings == null || strings.Length == 0)
            return defaultIfNotFound;

        if (string.IsNullOrWhiteSpace(strings[0]))
            return defaultIfNotFound;

        return (T)Convert.ChangeType(strings[0], typeof(T));
    }

    /// <summary>Gets the file name at the given index</summary>
    /// <param name="httpRequest">Http Request that holds the file</param>
    /// <param name="zeroBasedFileIndex">Zero based index of the file you desire</param>
    public string GetFileName(HttpRequest httpRequest, int zeroBasedFileIndex)
    {
        IFormFile postedFile = GetHttpPostedFile(httpRequest, zeroBasedFileIndex);
        if (postedFile == null || postedFile.Length <= 0)
            return string.Empty;

        // Using Path.GetFileName due to IE (not needed for Chrome)
        return Path.GetFileName(postedFile.FileName.Trim());
    }

    ///// <summary>Gets the MIME type of the file at the given index</summary>
    ///// <param name="httpRequest">Http Request that holds the file</param>
    ///// <param name="zeroBasedFileIndex">Zero based index of the file you desire</param>
    //public static string GetFileMimeType(HttpRequestBase httpRequest, int zeroBasedFileIndex)
    //{
    //    HttpPostedFileBase postedFile = GetHttpPostedFile(httpRequest, zeroBasedFileIndex);
    //    if (postedFile == null || postedFile.ContentLength <= 0)
    //        return string.Empty;

    //    return postedFile.ContentType;
    //}

    /// <summary>Gives you a file stream for the incoming file.  Should you dispose it?
    /// It's not necessary according the the official guidance</summary>
    /// <param name="httpRequest">Http Request that holds the file</param>
    /// <param name="zeroBasedFileIndex">Zero based index of the file you desire</param>
    /// <remarks>
    /// Note 1: https://docs.microsoft.com/en-us/dotnet/api/system.web.httppostedfile?redirectedfrom=MSDN&view=netframework-4.8
    ///         States, "Server resources that are allocated to buffer the uploaded file will be destroyed when the request ends." 
    /// Note 2: Interesting read https://stackoverflow.com/questions/37139995/should-i-dispose-fileupload-postedfile-inputstream
    /// </remarks>
    public Stream GetInputStream(HttpRequest httpRequest, int zeroBasedFileIndex)
    {
        IFormFile postedFile = GetHttpPostedFile(httpRequest, zeroBasedFileIndex);
        return postedFile.OpenReadStream();
    }


    ///// <summary>Gives you a byte array for the incoming file.</summary>
    ///// <param name="httpRequest">Http Request that holds the file</param>
    ///// <param name="zeroBasedFileIndex">Zero based index of the file you desire</param>
    //public static byte[] GetInputStreamAsByteArray(HttpRequestBase httpRequest, int zeroBasedFileIndex)
    //{
    //    HttpPostedFileBase postedFile = GetHttpPostedFile(httpRequest, zeroBasedFileIndex);
    //    if (postedFile == null || postedFile.ContentLength <= 0)
    //        return new byte[0];

    //    var contentLength = postedFile.ContentLength;
    //    var content = new byte[contentLength];
    //    postedFile.InputStream.Read(content, 0, contentLength);
    //    return content;
    //}

    ///// <summary>Gives you the file size for the incoming file.</summary>
    ///// <param name="httpRequest">Http Request that holds the file</param>
    ///// <param name="zeroBasedFileIndex">Zero based index of the file you desire</param>
    //public static int GetFileSizeInBytes(HttpRequestBase httpRequest, int zeroBasedFileIndex)
    //{
    //    HttpPostedFileBase postedFile = GetHttpPostedFile(httpRequest, zeroBasedFileIndex);
    //    if (postedFile == null || postedFile.ContentLength <= 0)
    //        return 0;

    //    // Using Path.GetFileName due to IE (not needed for Chrome)
    //    return postedFile.ContentLength;
    //}

    ///// <summary>Saves a file to a given path.</summary>
    ///// <param name="httpRequest">Http Request that holds the file</param>
    ///// <param name="zeroBasedFileIndex">Zero based index of the file you desire</param>
    ///// <param name="fileNameAndPath">Name of the file (includes path e.g. c:\temp\MyFile.csv)</param>
    //public static bool SaveFile(HttpRequestBase httpRequest, int zeroBasedFileIndex, string fileNameAndPath)
    //{
    //    HttpPostedFileBase postedFile = GetHttpPostedFile(httpRequest, zeroBasedFileIndex);
    //    if (postedFile == null || postedFile.ContentLength <= 0)
    //        return false;

    //    // Is the user trying to upload a new file or replace the existing files?
    //    string directoryName = Path.GetDirectoryName(fileNameAndPath);
    //    if (string.IsNullOrEmpty(directoryName) == false && Directory.Exists(directoryName) == false)
    //    {
    //        Directory.CreateDirectory(directoryName);
    //    }

    //    postedFile.SaveAs(fileNameAndPath);

    //    return true;
    //}


    /// <summary>Gets a HttpPostedFileBase object for a given file by index</summary>
    /// <param name="httpRequest">Http Request that holds the file</param>
    /// <param name="zeroBasedFileIndex">Zero based index of the file you desire</param>
    private IFormFile GetHttpPostedFile(HttpRequest httpRequest, int zeroBasedFileIndex)
    {
        if (httpRequest == null)
            return null;

        if (httpRequest.Form.Files.Count == 0 || zeroBasedFileIndex >= httpRequest.Form.Files.Count)
        {
            return null;
        }

        return httpRequest.Form.Files[zeroBasedFileIndex];
    }
}