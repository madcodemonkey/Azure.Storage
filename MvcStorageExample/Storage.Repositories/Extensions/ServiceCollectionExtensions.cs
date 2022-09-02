using Microsoft.Extensions.DependencyInjection;

namespace Storage.Repositories;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStorageRepositories(this IServiceCollection services,
        BlobStorageSettings blobStorageSettings, FileStorageSettings fileStorageSettings)
    {
        services.AddSingleton(blobStorageSettings);
        services.AddSingleton(fileStorageSettings);

        services.AddScoped<IBlobStorageRepository, BlobStorageRepository>();
        services.AddScoped<IFileStorageRepository, FileStorageRepository>();
        services.AddScoped<IFileNameParser, FileNameParser>();

        return services;
    }
}