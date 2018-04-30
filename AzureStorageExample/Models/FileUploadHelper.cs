using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace StorageExamples.Models
{
    public class FileUploadHelper
    {
        public static T GetFormValue<T>(HttpRequestBase httpRequest, string name, T defaultIfNotFound)
        {
            string[] strings = httpRequest.Form.GetValues(name);
            if (strings == null || strings.Length == 0)
                return defaultIfNotFound;
            if (string.IsNullOrWhiteSpace(strings[0]))
                return defaultIfNotFound;

            return (T)Convert.ChangeType(strings[0], typeof(T));
        }

        public static string GetFileName(HttpRequestBase httpRequest, int zeroBasedFileIndex)
        {
            HttpPostedFileBase postedFile = GetHttpPostedFile(httpRequest, zeroBasedFileIndex);
            if (postedFile == null || postedFile.ContentLength <= 0)
                return string.Empty;

            // Using Path.GetFileName due to IE (not needed for Chrome)
            return Path.GetFileName(postedFile.FileName.Trim());
        }

        public static string GetFileMimeType(HttpRequestBase httpRequest, int zeroBasedFileIndex)
        {
            HttpPostedFileBase postedFile = GetHttpPostedFile(httpRequest, zeroBasedFileIndex);
            if (postedFile == null || postedFile.ContentLength <= 0)
                return string.Empty;

            return postedFile.ContentType;
        }


        public static Stream GetInputStream(HttpRequestBase httpRequest, int zeroBasedFileIndex)
        {
            HttpPostedFileBase postedFile = GetHttpPostedFile(httpRequest, zeroBasedFileIndex);
            if (postedFile == null || postedFile.ContentLength <= 0)
                return null;

            var contentLength = postedFile.ContentLength;
            var content = new byte[contentLength];
            return postedFile.InputStream;
        }


        public static byte[] GetInputStreamAsByteArray(HttpRequestBase httpRequest, int zeroBasedFileIndex)
        {
            HttpPostedFileBase postedFile = GetHttpPostedFile(httpRequest, zeroBasedFileIndex);
            if (postedFile == null || postedFile.ContentLength <= 0)
                return new byte[0];

            var contentLength = postedFile.ContentLength;
            var content = new byte[contentLength];
            postedFile.InputStream.Read(content, 0, contentLength);
            return content;
        }

        public static int GetFileSizeInBytes(HttpRequestBase httpRequest, int zeroBasedFileIndex)
        {
            HttpPostedFileBase postedFile = GetHttpPostedFile(httpRequest, zeroBasedFileIndex);
            if (postedFile == null || postedFile.ContentLength <= 0)
                return 0;

            // Using Path.GetFileName due to IE (not needed for Chrome)
            return postedFile.ContentLength;
        }

        public static bool SaveFile(HttpRequestBase httpRequest, int zeroBasedFileIndex, string fileNameAndPath)
        {
            HttpPostedFileBase postedFile = GetHttpPostedFile(httpRequest, zeroBasedFileIndex);
            if (postedFile == null || postedFile.ContentLength <= 0)
                return false;

            // Is the user trying to upload a new file or replace the existing files?
            string directoryName = Path.GetDirectoryName(fileNameAndPath);
            if (string.IsNullOrEmpty(directoryName) == false && Directory.Exists(directoryName) == false)
            {
                Directory.CreateDirectory(directoryName);
            }

            postedFile.SaveAs(fileNameAndPath);

            return true;
        }


        private static HttpPostedFileBase GetHttpPostedFile(HttpRequestBase httpRequest, int zeroBasedFileIndex)
        {
            if (httpRequest == null)
                return null;
            if (httpRequest.Files.Count == 0 || zeroBasedFileIndex >= httpRequest.Files.Count)
            {
                return null;
            }

            return httpRequest.Files.Get(zeroBasedFileIndex);
        }
    }
}