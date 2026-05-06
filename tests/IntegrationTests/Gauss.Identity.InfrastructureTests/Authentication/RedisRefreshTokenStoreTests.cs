using AwesomeAssertions;
using Gauss.Identity.Application.Authentication.RefreshTokens;
using Gauss.Identity.Infrastructure.Authentication;
using Gauss.Identity.InfrastructureTests.Fixtures;
using Microsoft.Extensions.Options;

namespace Gauss.Identity.InfrastructureTests.Authentication;

public sealed class RedisRefreshTokenStoreTests(
    RedisTestFixture redisFixture)
    : IClassFixture<RedisTestFixture>
{
    [Fact(DisplayName = "Should store and retrieve refresh token session")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Authentication")]
    public async Task Should_Store_And_Retrieve_Refresh_Token_Session()
    {
        // Arrange
        var store = CreateStore();

        var session = CreateSession();

        // Act
        await store.StoreAsync(session);

        var persistedSession = await store.GetByHashAsync(session.RefreshTokenHash);

        // Assert
        persistedSession.Should().NotBeNull();

        persistedSession!.SessionId.Should().Be(session.SessionId);
        persistedSession.UserId.Should().Be(session.UserId);
        persistedSession.TenantId.Should().Be(session.TenantId);
        persistedSession.RefreshTokenHash.Should().Be(session.RefreshTokenHash);
        persistedSession.IssuedAtUtc.Should().Be(session.IssuedAtUtc);
        persistedSession.ExpiresAtUtc.Should().Be(session.ExpiresAtUtc);
        persistedSession.RotatedAtUtc.Should().Be(session.RotatedAtUtc);
        persistedSession.RevokedAtUtc.Should().Be(session.RevokedAtUtc);
        persistedSession.ReplacedBySessionId.Should().Be(session.ReplacedBySessionId);
    }

    [Fact(DisplayName = "Should return null when refresh token session does not exist")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Authentication")]
    public async Task Should_Return_Null_When_Refresh_Token_Session_Does_Not_Exist()
    {
        // Arrange
        var store = CreateStore();

        // Act
        var session = await store.GetByHashAsync("missing-refresh-token-hash");

        // Assert
        session.Should().BeNull();
    }

    [Fact(DisplayName = "Should delete refresh token session")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Authentication")]
    public async Task Should_Delete_Refresh_Token_Session()
    {
        // Arrange
        var store = CreateStore();

        var session = CreateSession();

        await store.StoreAsync(session);

        // Act
        await store.DeleteAsync(session.RefreshTokenHash);

        var persistedSession = await store.GetByHashAsync(session.RefreshTokenHash);

        // Assert
        persistedSession.Should().BeNull();
    }

    [Fact(DisplayName = "Should expire refresh token session using Redis TTL")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Authentication")]
    public async Task Should_Expire_Refresh_Token_Session_Using_Redis_Ttl()
    {
        // Arrange
        var store = CreateStore();

        var issuedAtUtc = DateTimeOffset.UtcNow;
        var expiresAtUtc = issuedAtUtc.AddSeconds(1);

        var session = CreateSession(
            issuedAtUtc,
            expiresAtUtc);

        // Act
        await store.StoreAsync(session);

        await Task.Delay(TimeSpan.FromSeconds(2));

        var persistedSession = await store.GetByHashAsync(session.RefreshTokenHash);

        // Assert
        persistedSession.Should().BeNull();
    }

    private RedisRefreshTokenStore CreateStore()
    {
        return new RedisRefreshTokenStore(
            redisFixture.Multiplexer,
            Options.Create(new RefreshTokenOptions
            {
                ExpirationMinutes = 10080
            }));
    }

    private static RefreshTokenSession CreateSession()
    {
        var issuedAtUtc = new DateTimeOffset(2026, 05, 04, 12, 0, 0, TimeSpan.Zero);
        var expiresAtUtc = issuedAtUtc.AddDays(7);

        return CreateSession(
            issuedAtUtc,
            expiresAtUtc);
    }

    private static RefreshTokenSession CreateSession(
        DateTimeOffset issuedAtUtc,
        DateTimeOffset expiresAtUtc)
    {
        return new RefreshTokenSession(
            SessionId: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            TenantId: Guid.NewGuid(),
            RefreshTokenHash: $"refresh-token-hash-{Guid.NewGuid():N}",
            IssuedAtUtc: issuedAtUtc,
            ExpiresAtUtc: expiresAtUtc);
    }
}
