using ConsoleMenuHelper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Storage.Repositories.Extensions;
using Storage.Repositories.Settings;
using System.Reflection;

try
{
    var menu = new ConsoleMenu();

    menu.AddDependencies(AddMyDependencies);
    menu.AddMenuItemViaReflection(Assembly.GetExecutingAssembly());

    await menu.DisplayMenuAsync("Main", "Main");

    Console.WriteLine("Done!");
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
    Console.WriteLine(ex.StackTrace);
}


static void AddMyDependencies(IServiceCollection serviceCollection)
{

    // IConfiguration requires: Microsoft.Extensions.Configuration NuGet package
    // AddJsonFile requires:    Microsoft.Extensions.Configuration.Json NuGet package
    // https://stackoverflow.com/a/46437144/97803
    var builder = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json")
        .AddUserSecrets(Assembly.GetExecutingAssembly());

    IConfiguration config = builder.Build();

    serviceCollection.AddSingleton<IConfiguration>(config);
    serviceCollection.AddStorageRepositories(GetBlobStorageSettings(config));
    serviceCollection.AddStorageServices();
}

static BlobStorageSettings GetBlobStorageSettings(IConfiguration config)
{
    return new BlobStorageSettings
    {
        ConnectionString = config["StorageConnectionString"],
        ContainerName = config["BlobContainerName"]
    };

}
 