using StackExchange.Redis;

namespace Gauss.Testing.Fixtures;

public sealed class RedisTestFixture : IAsyncLifetime
{
    private const string DefaultConnectionString =
        "localhost:6379,abortConnect=false";

    private ConnectionMultiplexer? _connectionMultiplexer;

    public string ConnectionString =>
        Environment.GetEnvironmentVariable("GAUSS_TEST_REDIS_CONNECTION_STRING")
        ?? DefaultConnectionString;

    public ConnectionMultiplexer Multiplexer =>
        _connectionMultiplexer
        ?? throw new InvalidOperationException("Redis connection multiplexer was not initialized.");

    public async Task InitializeAsync()
    {
        _connectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(
            ConnectionString);

        await FlushDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        await FlushDatabaseAsync();

        if (_connectionMultiplexer is not null)
        {
            await _connectionMultiplexer.CloseAsync();
            await _connectionMultiplexer.DisposeAsync();
        }
    }

    public async Task FlushDatabaseAsync()
    {
        if (_connectionMultiplexer is null)
        {
            return;
        }

        var endpoints = _connectionMultiplexer.GetEndPoints();

        foreach (var endpoint in endpoints)
        {
            var server = _connectionMultiplexer.GetServer(endpoint);

            await server.FlushDatabaseAsync();
        }
    }
}
