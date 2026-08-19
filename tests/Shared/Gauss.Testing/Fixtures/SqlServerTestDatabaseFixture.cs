using FluentMigrator.Runner;
using Gauss.Testing.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace Gauss.Testing.Fixtures;

public sealed class SqlServerTestDatabaseFixture : IAsyncLifetime
{
    private readonly string _databaseName = $"Gauss_Tests_{Guid.NewGuid():N}";

    public string ConnectionString => CreateConnectionString(_databaseName);

    private string MasterConnectionString => CreateConnectionString("master");

    public async Task InitializeAsync()
    {
        await CreateDatabaseAsync();

        RunMigrations();
    }

    public async Task DisposeAsync()
    {
        await DropDatabaseAsync();
    }

    private static string CreateConnectionString(string databaseName)
    {
        var host = TestConfiguration.GetOptional("GAUSS_TEST_SQLSERVER_HOST") ?? @".\SQLEXPRESS";
        var user = TestConfiguration.GetOptional("GAUSS_TEST_SQLSERVER_USER") ?? "sa";
        var password = TestConfiguration.GetRequired("GAUSS_TEST_SQLSERVER_PASSWORD");

        return $"Server={host};Database={databaseName};User ID={user};Password={password};TrustServerCertificate=True;MultipleActiveResultSets=true;Encrypt=True;";
    }

    private async Task CreateDatabaseAsync()
    {
        await using var connection = new SqlConnection(MasterConnectionString);

        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = $"""
            CREATE DATABASE [{_databaseName}];
            """;

        await command.ExecuteNonQueryAsync();
    }

    private void RunMigrations()
    {
        using var serviceProvider = new ServiceCollection()
            .AddFluentMigratorCore()
            .ConfigureRunner(runner =>
            {
                runner
                    .AddSqlServer()
                    .WithGlobalConnectionString(ConnectionString)
                    .ScanIn(typeof(Gauss.Database.Migrations.Program).Assembly)
                    .For.Migrations();
            })
            .AddLogging(logging => logging.AddFluentMigratorConsole())
            .BuildServiceProvider(validateScopes: false);

        using var scope = serviceProvider.CreateScope();

        var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();

        runner.MigrateUp();
    }

    private async Task DropDatabaseAsync()
    {
        await using var connection = new SqlConnection(MasterConnectionString);

        await connection.OpenAsync();

        await using var command = connection.CreateCommand();

        command.CommandText = $"""
            ALTER DATABASE [{_databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
            DROP DATABASE [{_databaseName}];
            """;

        await command.ExecuteNonQueryAsync();
    }
}
