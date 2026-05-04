using FluentMigrator.Runner;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace Gauss.Identity.ApiTests.Fixtures;

public sealed class SqlServerTestDatabaseFixture : IAsyncLifetime
{
    private const string DefaultSqlServerHost = @".\SQLEXPRESS";
    private const string DefaultSqlServerUser = "sa";
    private const string DefaultSqlServerPassword = "Asd123!!!";

    private readonly string _databaseName = $"Gauss_IdentityApiTests_{Guid.NewGuid():N}";

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
        var host = Environment.GetEnvironmentVariable("GAUSS_TEST_SQLSERVER_HOST")
            ?? DefaultSqlServerHost;

        var user = Environment.GetEnvironmentVariable("GAUSS_TEST_SQLSERVER_USER")
            ?? DefaultSqlServerUser;

        var password = Environment.GetEnvironmentVariable("GAUSS_TEST_SQLSERVER_PASSWORD")
            ?? DefaultSqlServerPassword;

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
