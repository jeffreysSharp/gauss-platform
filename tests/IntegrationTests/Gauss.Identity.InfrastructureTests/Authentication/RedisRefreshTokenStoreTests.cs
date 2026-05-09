using AwesomeAssertions;
using Gauss.Identity.Domain.RefreshTokens;
using Gauss.Identity.Infrastructure.Authentication;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Gauss.Identity.InfrastructureTests.Authentication;

public sealed class RedisRefreshTokenStoreTests
{
    private readonly IConnectionMultiplexer _redisConnection;
    private readonly RedisRefreshTokenStore _store;

    public RedisRefreshTokenStoreTests()
    {
        _redisConnection = ConnectionMultiplexer.Connect("localhost:6379");
        _store = new RedisRefreshTokenStore(
            _redisConnection,
            Options.Create(new RefreshTokenOptions
            {
                ExpirationMinutes = 10080
            }));
    }

    private static RefreshTokenSession CreateSession(Guid familyId, string refreshTokenHash)
    {
        var issuedAtUtc = DateTimeOffset.UtcNow;
        var expiresAtUtc = issuedAtUtc.AddDays(7);

        return new RefreshTokenSession(
            SessionId: Guid.NewGuid(),
            FamilyId: familyId,
            UserId: Guid.NewGuid(),
            TenantId: Guid.NewGuid(),
            RefreshTokenHash: refreshTokenHash,
            IssuedAtUtc: issuedAtUtc,
            ExpiresAtUtc: expiresAtUtc);
    }

    [Fact(DisplayName = "Should store and retrieve refresh token by hash")]
    public async Task Should_Store_And_GetByHash()
    {
        var session = CreateSession(Guid.NewGuid(), "token-hash-1");

        await _store.StoreAsync(session);

        var fetched = await _store.GetByHashAsync("token-hash-1");

        fetched.Should().NotBeNull();
        fetched!.SessionId.Should().Be(session.SessionId);
        fetched.RefreshTokenHash.Should().Be(session.RefreshTokenHash);
    }

    [Fact(DisplayName = "Should return null if refresh token hash does not exist")]
    public async Task Should_Return_Null_For_NonExisting_Hash()
    {
        var fetched = await _store.GetByHashAsync("non-existent-hash");
        fetched.Should().BeNull();
    }

    [Fact(DisplayName = "Should return refresh token sessions by family id")]
    public async Task Should_Return_Sessions_By_FamilyId()
    {
        var familyId = Guid.NewGuid();
        var session1 = CreateSession(familyId, "token1");
        var session2 = CreateSession(familyId, "token2");

        await _store.StoreAsync(session1);
        await _store.StoreAsync(session2);

        var sessions = await _store.GetByFamilyIdAsync(familyId);

        sessions.Should().HaveCount(2);
        sessions.Select(s => s.RefreshTokenHash)
            .Should()
            .BeEquivalentTo("token1", "token2");
    }

    [Fact(DisplayName = "Should revoke refresh token family")]
    public async Task Should_Revoke_Family()
    {
        var familyId = Guid.NewGuid();
        var revokedAtUtc = DateTimeOffset.UtcNow;

        var session1 = CreateSession(familyId, "token1");
        var session2 = CreateSession(familyId, "token2");

        await _store.StoreAsync(session1);
        await _store.StoreAsync(session2);

        await _store.RevokeFamilyAsync(familyId, revokedAtUtc);

        var sessions = await _store.GetByFamilyIdAsync(familyId);

        sessions.Should().HaveCount(2);
        sessions.Should().OnlyContain(s =>
            s.IsRevoked &&
            s.RevokedAtUtc == revokedAtUtc);
    }

    [Fact(DisplayName = "Should handle empty family gracefully")]
    public async Task Should_Handle_Empty_Family()
    {
        var sessions = await _store.GetByFamilyIdAsync(Guid.NewGuid());
        sessions.Should().BeEmpty();

        await _store.RevokeFamilyAsync(Guid.NewGuid(), DateTimeOffset.UtcNow);
    }
}
