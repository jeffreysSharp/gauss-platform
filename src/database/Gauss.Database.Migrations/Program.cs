using FluentMigrator.Runner;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Gauss.Database.Migrations;

internal static class Program
{
    public static int Main(string[] args)
    {
        var connectionString = GetConnectionString(args);

        using var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(services =>
            {
                services
                    .AddFluentMigratorCore()
                    .ConfigureRunner(runner =>
                    {
                        runner
                            .AddSqlServer()
                            .WithGlobalConnectionString(connectionString)
                            .ScanIn(typeof(Program).Assembly).For.Migrations();
                    })
                    .AddLogging(logging => logging.AddFluentMigratorConsole());
            })
            .Build();

        using var scope = host.Services.CreateScope();

        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

        runner.MigrateUp();

        return 0;
    }

    private static string GetConnectionString(string[] args)
    {
        var connectionStringArgument = args.FirstOrDefault(argument =>
            argument.StartsWith("--connection-string=", StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(connectionStringArgument))
        {
            return connectionStringArgument["--connection-string=".Length..];
        }

        var environmentConnectionString = Environment.GetEnvironmentVariable(
            "GAUSS_SQL_CONNECTION_STRING");

        if (!string.IsNullOrWhiteSpace(environmentConnectionString))
        {
            return environmentConnectionString;
        }

        throw new InvalidOperationException(
            "Database connection string was not provided. Use --connection-string=... or GAUSS_SQL_CONNECTION_STRING.");
    }
}
