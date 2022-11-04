using Microsoft.Extensions.DependencyInjection;
using Storage.Repositories.Abstractions;
using Storage.Repositories.Repository;
using Storage.Repositories.Settings;
using Storage.Repositories.Utility;

namespace Storage.Repositories.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStorageRepositories(this IServiceCollection services, BlobStorageSettings blobStorageSettings)
    {
        services.AddSingleton(blobStorageSettings);

        services.AddScoped<IBlobStorageRepository, BlobStorageRepository>();
        services.AddScoped<IFileNameParser, FileNameParser>();

        return services;
    }
}