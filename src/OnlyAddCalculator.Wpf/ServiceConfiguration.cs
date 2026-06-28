using System.IO;
using Microsoft.Extensions.DependencyInjection;
using OnlyAddCalculator.Application;
using OnlyAddCalculator.Core;
using OnlyAddCalculator.Persistence;
using OnlyAddCalculator.Wpf.Resources;

namespace OnlyAddCalculator.Wpf;

public static class ServiceConfiguration
{
    public static ServiceProvider CreateServiceProvider()
    {
        var services = new ServiceCollection();

        services.AddSingleton<AdditionCalculator>();
        services.AddSingleton<IAppStateStore>(_ => new JsonAppStateStore(CreateStateFilePath()));
        services.AddSingleton<MainWindowFactory>();

        return services.BuildServiceProvider();
    }

    private static string CreateStateFilePath()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        return Path.Combine(appDataPath, StorageResources.StateStorageDirectoryName, StorageResources.StateStorageFileName);
    }
}
