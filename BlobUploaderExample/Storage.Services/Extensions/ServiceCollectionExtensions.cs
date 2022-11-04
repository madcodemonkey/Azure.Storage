using Microsoft.Extensions.DependencyInjection;
using Storage.Services;

namespace Storage.Repositories.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddStorageServices(this IServiceCollection services)
    {

        services.AddScoped<IUploadService, UploadService>();

        return services;
    }
}