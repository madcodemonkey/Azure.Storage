using MvcStorageExample.Utility;
using Storage.Repositories;

namespace MvcStorageExample.Extensions;

/// <summary>This adds services and other items that are specific to this application that are NOT reusable in another application.</summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMvcStorageWebServices(this IServiceCollection services, IConfiguration config)
    {

        var blobStorageSettings = new BlobStorageSettings
        {
            ConnectionString = config["StorageConnectionString"],
            ContainerName =  config["BlobContainerName"]
        };

        var fileStorageSettings = new FileStorageSettings
        {
            ConnectionString = config["StorageConnectionString"],
            ShareName = config["ShareName"]
        };

        services.AddStorageRepositories(blobStorageSettings, fileStorageSettings);

        services.AddScoped<IFileHelperService, FileHelperService>();

        return services;

    }
         
}