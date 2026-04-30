namespace Gauss.Identity.Api.Installers;

public sealed class HealthChecksInstaller : IInstaller
{
    public void InstallServices(
        IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddHealthChecks()
            .AddSqlServer(
                connectionString: configuration.GetRequiredSection("Identity:Persistence")
                    .GetValue<string>("ConnectionString")
                    ?? throw new InvalidOperationException("Identity persistence connection string was not configured."),
                name: "sqlserver",
                tags: ["ready"]);
    }
}
