using FluentMigrator.Runner;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;

namespace Gauss.Identity.ApiTests.Fixtures;

public sealed class SqlServerTestDatabaseFixture : IAsyncLifetime
{
    private readonly string _databaseName = $"Gauss_IdentityApiTests_{Guid.NewGuid():N}";

    public string ConnectionString =>
        $"Server=.\\SQLEXPRESS;Database={_databaseName};User ID=sa;Password=Asd123!!!;TrustServerCertificate=True;MultipleActiveResultSets=true;Encrypt=True;";

    private string MasterConnectionString =>
        "Server=.\\SQLEXPRESS;Database=master;User ID=sa;Password=Asd123!!!;TrustServerCertificate=True;MultipleActiveResultSets=true;Encrypt=True;";

    public async Task InitializeAsync()
    {
        await CreateDatabaseAsync();
        RunMigrations();
    }

    public async Task DisposeAsync()
    {
        await DropDatabaseAsync();
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
