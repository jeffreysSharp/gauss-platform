using System.Reflection;

namespace Gauss.Identity.Api.Installers;

public static class InstallerExtensions
{
    public static IServiceCollection InstallServices(
        this IServiceCollection services,
        IConfiguration configuration,
        params Assembly[] assemblies)
    {
        var installers = assemblies
            .SelectMany(assembly => assembly.ExportedTypes)
            .Where(type =>
                typeof(IInstaller).IsAssignableFrom(type) &&
                type is { IsInterface: false, IsAbstract: false })
            .Select(Activator.CreateInstance)
            .Cast<IInstaller>()
            .OrderBy(installer => installer.Order);

        foreach (var installer in installers)
        {
            installer.InstallServices(services, configuration);
        }

        return services;
    }
}
