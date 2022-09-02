using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;

namespace Storage.Repositories;

public class FileStorageRepository : IFileStorageRepository
{
    private readonly IFileNameParser _fileNameParser;
    private readonly ShareClient _share;

    /// <summary>Constructor</summary>>
    public FileStorageRepository(FileStorageSettings settings, IFileNameParser fileNameParser)
    {
        _fileNameParser = fileNameParser;
        _share = new ShareClient(settings.ConnectionString, settings.ShareName);
    }

    /// <summary>Creates a wrapper around the file so that you can perform actions on it (doesn't do anything to the server).</summary>
    /// <param name="fileName">The file name (the path will be extracted from the file name)</param>
    public ShareFileClient CreateClient(string fileName)
    {
        var parsedName = _fileNameParser.Parse(fileName);
        return CreateClient(parsedName.DirectoryName, parsedName.FileName);
    }

    /// <summary>Creates a wrapper around the file so that you can perform actions on it (doesn't do anything to the server).</summary>
    /// <param name="directoryName">The directory of interest or empty/null if you want the root directory</param>
    /// <param name="fileName">The file name without a path</param>
    public ShareFileClient CreateClient(string directoryName, string fileName)
    {
        ShareDirectoryClient directory = string.IsNullOrWhiteSpace(directoryName) ?
            _share.GetRootDirectoryClient() :
            _share.GetDirectoryClient(directoryName);

        return directory.GetFileClient(fileName);
    }

    /// <summary>Delete a file</summary>
    /// <param name="fileName">The file name (the path will be extracted from the file name)</param>
    public async Task DeleteAsync(string fileName)
    {
        var parsedName = _fileNameParser.Parse(fileName);
        await DeleteAsync(parsedName.DirectoryName, parsedName.FileName);
    }

    /// <summary>Delete a file</summary>
    /// <param name="directoryName">The directory of interest or empty/null if you want the root directory</param>
    /// <param name="fileName">The file name without a path</param>
    public async Task DeleteAsync(string directoryName, string fileName)
    {
        var shareFileClient = CreateClient(directoryName, fileName);

        // Delete the file if it exists.  If you don't check and it doesn't exist, you will get a 404 error.
        if (await shareFileClient.ExistsAsync())
            await shareFileClient.DeleteAsync();
    }


    /// <summary>Returns a stream so you can download a file</summary>
    /// <param name="fileName">The file name (the path will be extracted from the file name)</param>
    public async Task<Stream> DownloadAsync(string fileName)
    {
        var parsedName = _fileNameParser.Parse(fileName);
        return await DownloadAsync(parsedName.DirectoryName, parsedName.FileName);
    }

    /// <summary>Returns a stream so you can download a file</summary>
    /// <param name="directoryName">The directory of interest or empty/null if you want the root directory</param>
    /// <param name="fileName">The file name without a path</param>
    public async Task<Stream> DownloadAsync(string directoryName, string fileName)
    {
        ShareFileClient file = CreateClient(directoryName, fileName);

        //// Download the file
        ShareFileDownloadInfo download = await file.DownloadAsync();
        return download.Content;
    }

    /// <summary>List files</summary>
    /// <param name="directoryName">The directory of interest or null if you want the root directory</param>
    /// <param name="returnDirectoriesAndFiles">If true, you will get both directories and file.  If false, you will get only files.</param>
    /// <param name="cancellationToken"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public async Task<List<ShareFileItem>> ListFilesAsync(string directoryName,
        bool returnDirectoriesAndFiles, CancellationToken cancellationToken = default,
        ShareDirectoryGetFilesAndDirectoriesOptions options = null)
    {
        var result = new List<ShareFileItem>();

        ShareDirectoryClient directory = string.IsNullOrWhiteSpace(directoryName) ?
            _share.GetRootDirectoryClient() :
            _share.GetDirectoryClient(directoryName);


        // For >= C# 8.0 use:
        //await foreach (ShareFileItem fileItem in directory.GetFilesAndDirectoriesAsync(options, cancellationToken))
        //{
        //    result.Add(fileItem);
        //}

        // For < C# 8.0 use:
        AsyncPageable<ShareFileItem> filePages = directory.GetFilesAndDirectoriesAsync(options, cancellationToken);
        IAsyncEnumerator<ShareFileItem> enumerator = filePages.GetAsyncEnumerator();
        try
        {
            while (await enumerator.MoveNextAsync())
            {
                if (returnDirectoriesAndFiles == false && enumerator.Current.IsDirectory) continue;
                result.Add(enumerator.Current);
            }
        }
        finally
        {
            await enumerator.DisposeAsync();
        }


        return result;
    }


    /// <summary>Uploads a file</summary>
    /// <param name="fileStream">The file stream to upload.</param>
    /// <param name="fileName">The file name (the path will be extracted from the file name)</param>
    public async Task UploadAsync(Stream fileStream, string fileName)
    {
        var parsedName = _fileNameParser.Parse(fileName);
        await UploadAsync(fileStream, parsedName.DirectoryName, parsedName.FileName);
    }


    /// <summary>Uploads a file</summary>
    /// <param name="fileStream">The file stream to upload.</param>
    /// <param name="directoryName">The directory of interest or empty/null if you want the root directory</param>
    /// <param name="fileName">The file name without a path</param>
    public async Task UploadAsync(Stream fileStream, string directoryName, string fileName)
    {
        // Get a reference to a directory and create it necessary
        ShareDirectoryClient directory;
        if (string.IsNullOrWhiteSpace(directoryName))
        {
            directory = _share.GetRootDirectoryClient();
        }
        else
        {
            directory = _share.GetDirectoryClient(directoryName);
            if (await directory.ExistsAsync() == false)
                await directory.CreateAsync();
        }


        ShareFileClient file = directory.GetFileClient(fileName);
        await file.CreateAsync(fileStream.Length);
        await file.UploadRangeAsync(new HttpRange(0, fileStream.Length), fileStream);
    }
}