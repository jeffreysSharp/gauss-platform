using System.Text.Json;
using Gauss.Identity.Application.Abstractions.Authentication;
using Gauss.Identity.Application.Authentication.RefreshTokens;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Gauss.Identity.Infrastructure.Authentication;

public sealed class RedisRefreshTokenStore(
    IConnectionMultiplexer connectionMultiplexer,
    IOptions<RefreshTokenOptions> refreshTokenOptions)
    : IRefreshTokenStore
{
    private const string KeyPrefix = "identity:refresh-token:";

    private static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly IDatabase _database = connectionMultiplexer.GetDatabase();

    private readonly RefreshTokenOptions _refreshTokenOptions = refreshTokenOptions.Value;

    public async Task StoreAsync(
        RefreshTokenSession session,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);

        var key = CreateKey(session.RefreshTokenHash);

        var value = JsonSerializer.Serialize(
            session,
            JsonSerializerOptions);

        var timeToLive = CalculateTimeToLive(session);

        if (timeToLive <= TimeSpan.Zero)
        {
            return;
        }

        await _database.StringSetAsync(
            key,
            value,
            timeToLive);
    }

    public async Task<RefreshTokenSession?> GetByHashAsync(
     string refreshTokenHash,
     CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshTokenHash);

        var key = CreateKey(refreshTokenHash);

        var value = await _database.StringGetAsync(key);

        if (value.IsNullOrEmpty)
        {
            return null;
        }

        var serializedSession = value.ToString();

        return JsonSerializer.Deserialize<RefreshTokenSession>(
            serializedSession,
            JsonSerializerOptions);
    }

    public async Task DeleteAsync(
        string refreshTokenHash,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshTokenHash);

        var key = CreateKey(refreshTokenHash);

        await _database.KeyDeleteAsync(key);
    }

    public async Task UpdateAsync(
    RefreshTokenSession session,
    CancellationToken cancellationToken = default)
    {
        await StoreAsync(
            session,
            cancellationToken);
    }

    public Task<IReadOnlyCollection<RefreshTokenSession>> GetByFamilyIdAsync(
    Guid familyId,
    CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyCollection<RefreshTokenSession>>([]);
    }

    public Task RevokeFamilyAsync(
        Guid familyId,
        DateTimeOffset revokedAtUtc,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    private TimeSpan CalculateTimeToLive(
        RefreshTokenSession session)
    {
        var ttlFromSession = session.ExpiresAtUtc - session.IssuedAtUtc;

        if (ttlFromSession > TimeSpan.Zero)
        {
            return ttlFromSession;
        }

        return TimeSpan.FromMinutes(_refreshTokenOptions.ExpirationMinutes);
    }

    private static string CreateKey(
        string refreshTokenHash)
    {
        return $"{KeyPrefix}{refreshTokenHash}";
    }
}
