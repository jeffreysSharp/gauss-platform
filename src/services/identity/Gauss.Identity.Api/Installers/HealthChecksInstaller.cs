using Gauss.Identity.Infrastructure.Persistence;

namespace Gauss.Identity.Api.Installers;

public sealed class HealthChecksInstaller : IInstaller
{
    public void InstallServices(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetValue<string>(
            $"{IdentityPersistenceOptions.SectionName}:ConnectionString");

        var healthChecksBuilder = services.AddHealthChecks();

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            healthChecksBuilder.AddSqlServer(
                connectionString: connectionString,
                name: "sqlserver",
                tags: ["ready"]);
        }
    }
}
