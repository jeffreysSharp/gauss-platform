using System.Text.Json;
using Gauss.Identity.Application.Abstractions.Authentication;
using Gauss.Identity.Domain.RefreshTokens;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Gauss.Identity.Infrastructure.Authentication;

public sealed class RedisRefreshTokenStore(
    IConnectionMultiplexer connectionMultiplexer,
    IOptions<RefreshTokenOptions> refreshTokenOptions)
    : IRefreshTokenStore
{
    private const string SessionKeyPrefix = "identity:refresh-token:";
    private const string FamilyKeyPrefix = "identity:refresh-token-family:";

    private static readonly JsonSerializerOptions JsonSerializerOptions =
        new(JsonSerializerDefaults.Web);

    private readonly IDatabase _database = connectionMultiplexer.GetDatabase();

    private readonly RefreshTokenOptions _refreshTokenOptions = refreshTokenOptions.Value;

    public async Task StoreAsync(
        RefreshTokenSession session,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);

        var timeToLive = CalculateTimeToLive(session);

        if (timeToLive <= TimeSpan.Zero)
        {
            return;
        }

        var sessionKey = CreateSessionKey(session.RefreshTokenHash);
        var familyKey = CreateFamilyKey(session.FamilyId);

        var value = JsonSerializer.Serialize(
            session,
            JsonSerializerOptions);

        await _database.StringSetAsync(
            sessionKey,
            value,
            timeToLive);

        await _database.SetAddAsync(
            familyKey,
            session.RefreshTokenHash);

        await ExtendFamilyKeyExpirationAsync(
            familyKey,
            timeToLive);
    }

    public async Task<RefreshTokenSession?> GetByHashAsync(
        string refreshTokenHash,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshTokenHash);

        var key = CreateSessionKey(refreshTokenHash);

        var value = await _database.StringGetAsync(key);

        if (value.IsNullOrEmpty)
        {
            return null;
        }

        return JsonSerializer.Deserialize<RefreshTokenSession>(
            value.ToString(),
            JsonSerializerOptions);
    }

    public async Task UpdateAsync(
        RefreshTokenSession session,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(session);

        await StoreAsync(
            session,
            cancellationToken);
    }

    public async Task<IReadOnlyCollection<RefreshTokenSession>> GetByFamilyIdAsync(
        Guid familyId,
        CancellationToken cancellationToken = default)
    {
        if (familyId == Guid.Empty)
        {
            return [];
        }

        var familyKey = CreateFamilyKey(familyId);

        var refreshTokenHashes = await _database.SetMembersAsync(familyKey);

        if (refreshTokenHashes.Length == 0)
        {
            return [];
        }

        var sessions = new List<RefreshTokenSession>();

        foreach (var refreshTokenHash in refreshTokenHashes)
        {
            if (refreshTokenHash.IsNullOrEmpty)
            {
                continue;
            }

            var session = await GetByHashAsync(
                refreshTokenHash.ToString(),
                cancellationToken);

            if (session is not null)
            {
                sessions.Add(session);
            }
        }

        return sessions;
    }

    public async Task RevokeFamilyAsync(
        Guid familyId,
        DateTimeOffset revokedAtUtc,
        CancellationToken cancellationToken = default)
    {
        if (familyId == Guid.Empty)
        {
            return;
        }

        var sessions = await GetByFamilyIdAsync(
            familyId,
            cancellationToken);

        foreach (var session in sessions)
        {
            var revokedSession = session.Revoke(revokedAtUtc);

            await UpdateAsync(
                revokedSession,
                cancellationToken);
        }
    }

    private async Task ExtendFamilyKeyExpirationAsync(
        RedisKey familyKey,
        TimeSpan timeToLive)
    {
        var currentTimeToLive = await _database.KeyTimeToLiveAsync(familyKey);

        if (currentTimeToLive is null || currentTimeToLive < timeToLive)
        {
            await _database.KeyExpireAsync(
                familyKey,
                timeToLive);
        }
    }

    private TimeSpan CalculateTimeToLive(
        RefreshTokenSession session)
    {
        var timeToLive = session.ExpiresAtUtc - DateTimeOffset.UtcNow;

        if (timeToLive > TimeSpan.Zero)
        {
            return timeToLive;
        }

        return TimeSpan.FromMinutes(_refreshTokenOptions.ExpirationMinutes);
    }

    private static string CreateSessionKey(
        string refreshTokenHash)
    {
        return $"{SessionKeyPrefix}{refreshTokenHash}";
    }

    private static string CreateFamilyKey(
        Guid familyId)
    {
        return $"{FamilyKeyPrefix}{familyId:N}";
    }
}
