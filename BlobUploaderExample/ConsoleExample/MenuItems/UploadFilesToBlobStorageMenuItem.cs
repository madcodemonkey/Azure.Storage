using System.Reflection;
using ConsoleMenuHelper;
using Storage.Services;

namespace ConsoleExample.MenuItems;

[ConsoleMenuItem("Main")]
public class UploadFilesToBlobStorageMenuItem : IConsoleMenuItem
{
    private readonly IPromptHelper _promptHelper;
    private readonly IUploadService _uploadService;

    /// <summary>Constructor</summary>
    public UploadFilesToBlobStorageMenuItem(IPromptHelper promptHelper, IUploadService uploadService)
    {
        _promptHelper = promptHelper;
        _uploadService = uploadService;
    }

    public async Task<ConsoleMenuItemResponse> WorkAsync()
    {
        ////////// Find the upload files
        string uploadDirectoryNameEndsWith = "UploadFiles";

        string? uploadDirectory = FindUploadFilesDirectory(uploadDirectoryNameEndsWith);
        if (uploadDirectory == null) {
            Console.WriteLine($"Upload directory that ends with {uploadDirectoryNameEndsWith} was not found.");
            return new ConsoleMenuItemResponse(false, false);
        }
        
        if (_promptHelper.GetYorN($"Do you want to upload the files in the '{uploadDirectory}' directory", true) == false)
            return new ConsoleMenuItemResponse(false, false);

        ////////// Upload the files
        List<string> roles = new List<string>() { "admin", "member", "non-member" };
        await _uploadService.UploadFilesAsync(uploadDirectory, roles, "admin");


        Console.WriteLine("-------------------------------");

        return new ConsoleMenuItemResponse(false, false);
    }

    public string ItemText => "Upload files to blob storage";

    /// <summary>Optional data from the attribute.</summary>
    public string AttributeData { get; set; } = string.Empty;



    /// <summary>Start at the BIN directory where the exe is located and keep
    /// looking for folders that end with the prescribed text.  Once found,
    /// return the directory and exit; otherwise, return null.</summary>
    /// <param name="endsWithText">The directory name should end with this (you can put the entire directory name)</param>
    /// <returns>A directory that ends with teh prescribed text or null.</returns>
    private string? FindUploadFilesDirectory(string endsWithText = "UploadFiles")
    {
        string currentDirectorName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        if (string.IsNullOrWhiteSpace(currentDirectorName))
            return null;

        while (true)
        {
            if (currentDirectorName.EndsWith(endsWithText)) break;  // FOUND IT! EXIT!

            var childDirectories = Directory.GetDirectories(currentDirectorName);
            foreach (string childDirectoryName in childDirectories)
            {
                if (childDirectoryName.EndsWith(endsWithText))
                {
                    return childDirectoryName;   // FOUND IT! EXIT!
                }
            }

            // If the current directory is the root, the call to GetParent will return null and we can exit!
            var directoryInfo = Directory.GetParent(currentDirectorName);
            if (directoryInfo == null)
                break;

            // No? Go up a level
            currentDirectorName = directoryInfo.FullName;
        }

        // Did we find it?
        return currentDirectorName.EndsWith(endsWithText) ? currentDirectorName : null;
    }
}