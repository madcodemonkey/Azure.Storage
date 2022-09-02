using System;

namespace Storage.Repositories
{
    public class ParsedFileName
    {
        public string DirectoryName { get; set; }
        public string FileName { get; set; }
    }

    public class FileNameParser
    {
        public static ParsedFileName Parse(string fileName)
        {
            var result = new ParsedFileName();

            int indexOfBackSlash = fileName.LastIndexOf("\\");
            int indexOfSlash = fileName.LastIndexOf("/");

            // If there isn't a path in the file name, the file should be in the current directory.
            if (indexOfBackSlash == -1 && indexOfSlash == -1)
            {
                result.DirectoryName = string.Empty;
                result.FileName = fileName;
            }
            else
            {
                result.FileName = indexOfBackSlash != -1 ?
                    fileName.Substring(indexOfBackSlash + 1) :
                    fileName.Substring(indexOfSlash + 1);

                result.DirectoryName = indexOfBackSlash != -1 ?
                    GetSubDirectoryPath("\\", indexOfBackSlash, fileName) :
                    GetSubDirectoryPath("/", indexOfSlash, fileName);
            }

            return result;
        }

        
        /// <summary>Get a sub-directory path.</summary>
        /// <param name="slashType"></param>
        /// <param name="indexOfSlash"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private static string GetSubDirectoryPath(string slashType, int indexOfSlash, string fileName)
        {
            // Remove the filename from the path
            string result = fileName.Substring(0, indexOfSlash);

            // The CloudFileDirectory.GetDirectoryReference method cannot handle beginning slashes of either kind.
            if (result.StartsWith(slashType))
                result = result.Substring(1);

            // The CloudFileDirectory.GetDirectoryReference method cannot handle trailing slashes of either kind.
            if (result.EndsWith(slashType))
                result = result.Substring(0, slashType.Length - 1);

            return result;
        }
    }
}