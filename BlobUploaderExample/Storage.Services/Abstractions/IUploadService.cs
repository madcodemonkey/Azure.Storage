namespace Storage.Services;

public interface IUploadService
{
    Task UploadFilesAsync(string directoryName, List<string> roles, string? mandatoryRole = null);
}