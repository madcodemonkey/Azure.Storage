using Storage.Repositories.Abstractions;
using System.Text.Json;

namespace Storage.Services
{
    public class UploadService : IUploadService
    {
        private readonly IBlobStorageRepository _blobStorageRepository;
        private Random _random;

        public UploadService(IBlobStorageRepository blobStorageRepository)
        {
            _blobStorageRepository = blobStorageRepository;
            _random = new Random(DateTime.Now.Millisecond);
        }

        public async Task UploadFilesAsync(string directoryName, List<string> roles, string? mandatoryRole = null)
        {
            var files = Directory.GetFiles(directoryName);

            foreach (var fileName in files)
            {
                var justTheFileName = Path.GetFileName(fileName);

                using (var fs = File.OpenRead(fileName))
                {
                    var metadataList = new Dictionary<string, string>();
                    metadataList.Add("id", Guid.NewGuid().ToString());
                    metadataList.Add("roles", GetRandomRoles(roles, mandatoryRole));

                    if (await _blobStorageRepository.ExistsAsync(justTheFileName))
                    {
                        Console.WriteLine($"!!Skipping!! '{fileName}' because it already exists in the blob container!");
                    }
                    else
                    {
                        Console.WriteLine($"Start uploading '{fileName}'...");
                        await _blobStorageRepository.UploadAsync(justTheFileName, fs, metadataList);
                        Console.WriteLine($"Finished uploading '{fileName}'");
                    }
                }
            }
        }

        private string GetRandomRoles(List<string> roles, string? mandatoryRole)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(mandatoryRole) == false)
                result.Add(mandatoryRole);

            int itemsToAdd = _random.Next(1, roles.Count);
            for (int i = 0; i < itemsToAdd; i++)
            {
                int itemIndex = _random.Next(0, roles.Count - 1);
                string roleToAdd = roles[itemIndex];

                if (result.Any(w => w == roleToAdd) == false)
                {
                    result.Add(roleToAdd);
                }
            }

            return JsonSerializer.Serialize(result);
        }


    }
}