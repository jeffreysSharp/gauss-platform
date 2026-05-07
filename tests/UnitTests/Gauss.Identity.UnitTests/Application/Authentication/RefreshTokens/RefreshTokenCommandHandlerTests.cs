using AwesomeAssertions;
using Gauss.Identity.Application.Abstractions.Authentication;
using Gauss.Identity.Application.Abstractions.Persistence;
using Gauss.Identity.Application.Abstractions.Time;
using Gauss.Identity.Application.Authentication.RefreshTokens;
using Gauss.Identity.Domain.Tenants;
using Gauss.Identity.Domain.Users;
using Gauss.Identity.Domain.Users.ValueObjects;

namespace Gauss.Identity.UnitTests.Application.Authentication.RefreshTokens;

public sealed class RefreshTokenCommandHandlerTests
{
    [Fact(DisplayName = "Should refresh tokens when refresh token is valid")]
    [Trait("Layer", "Application")]
    [Trait("Category", "UseCases")]
    public async Task Should_Refresh_Tokens_When_RefreshToken_Is_Valid()
    {
        // Arrange
        var user = CreateActiveUser();
        var dateTimeProvider = new FakeDateTimeProvider();

        var currentSession = CreateActiveSession(
            user,
            dateTimeProvider.UtcNow);

        var userRepository = new FakeUserRepository
        {
            User = user
        };

        var accessTokenProvider = new FakeAccessTokenProvider();
        var refreshTokenGenerator = new FakeRefreshTokenGenerator();
        var refreshTokenHasher = new FakeRefreshTokenHasher();
        var refreshTokenStore = new FakeRefreshTokenStore
        {
            Session = currentSession
        };

        var handler = CreateHandler(
            userRepository: userRepository,
            accessTokenProvider: accessTokenProvider,
            refreshTokenGenerator: refreshTokenGenerator,
            refreshTokenHasher: refreshTokenHasher,
            refreshTokenStore: refreshTokenStore,
            dateTimeProvider: dateTimeProvider);

        var command = new RefreshTokenCommand("current-refresh-token-value");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();

        result.Value.AccessToken.Should().Be(accessTokenProvider.AccessToken.Value);
        result.Value.TokenType.Should().Be(accessTokenProvider.AccessToken.TokenType);
        result.Value.ExpiresAtUtc.Should().Be(accessTokenProvider.AccessToken.ExpiresAtUtc);
        result.Value.RefreshToken.Should().Be(refreshTokenGenerator.RefreshToken.Value);
        result.Value.RefreshTokenExpiresAtUtc.Should().Be(refreshTokenGenerator.RefreshToken.ExpiresAtUtc);

        refreshTokenHasher.HashedRefreshTokens.Should().Contain("current-refresh-token-value");
        refreshTokenHasher.HashedRefreshTokens.Should().Contain(refreshTokenGenerator.RefreshToken.Value);

        refreshTokenStore.LastHashChecked.Should().Be("hashed-current-refresh-token-value");
        userRepository.LastUserIdChecked.Should().Be(user.Id);
        accessTokenProvider.LastUser.Should().Be(user);
        refreshTokenGenerator.LastIssuedAtUtc.Should().Be(dateTimeProvider.UtcNow);

        refreshTokenStore.StoredSession.Should().NotBeNull();
        refreshTokenStore.StoredSession!.UserId.Should().Be(user.Id.Value);
        refreshTokenStore.StoredSession.TenantId.Should().Be(user.TenantId.Value);
        refreshTokenStore.StoredSession.RefreshTokenHash.Should().Be("hashed-new-refresh-token-value");
        refreshTokenStore.StoredSession.IssuedAtUtc.Should().Be(dateTimeProvider.UtcNow);
        refreshTokenStore.StoredSession.ExpiresAtUtc.Should().Be(refreshTokenGenerator.RefreshToken.ExpiresAtUtc);

        refreshTokenStore.DeletedHash.Should().Be(currentSession.RefreshTokenHash);
    }

    [Fact(DisplayName = "Should return invalid token when refresh token session does not exist")]
    [Trait("Layer", "Application")]
    [Trait("Category", "UseCases")]
    public async Task Should_Return_InvalidToken_When_RefreshTokenSession_Does_Not_Exist()
    {
        // Arrange
        var refreshTokenStore = new FakeRefreshTokenStore
        {
            Session = null
        };

        var handler = CreateHandler(
            refreshTokenStore: refreshTokenStore);

        var command = new RefreshTokenCommand("missing-refresh-token-value");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(RefreshTokenErrors.InvalidToken);

        refreshTokenStore.LastHashChecked.Should().Be("hashed-missing-refresh-token-value");
        refreshTokenStore.StoredSession.Should().BeNull();
        refreshTokenStore.DeletedHash.Should().BeNull();
    }

    [Fact(DisplayName = "Should return invalid token when refresh token session is expired")]
    [Trait("Layer", "Application")]
    [Trait("Category", "UseCases")]
    public async Task Should_Return_InvalidToken_When_RefreshTokenSession_Is_Expired()
    {
        // Arrange
        var dateTimeProvider = new FakeDateTimeProvider();

        var expiredSession = new RefreshTokenSession(
            SessionId: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            TenantId: Guid.NewGuid(),
            RefreshTokenHash: "hashed-current-refresh-token-value",
            IssuedAtUtc: dateTimeProvider.UtcNow.AddDays(-10),
            ExpiresAtUtc: dateTimeProvider.UtcNow.AddMinutes(-1));

        var refreshTokenStore = new FakeRefreshTokenStore
        {
            Session = expiredSession
        };

        var handler = CreateHandler(
            refreshTokenStore: refreshTokenStore,
            dateTimeProvider: dateTimeProvider);

        var command = new RefreshTokenCommand("current-refresh-token-value");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(RefreshTokenErrors.InvalidToken);

        refreshTokenStore.StoredSession.Should().BeNull();
        refreshTokenStore.DeletedHash.Should().BeNull();
    }

    [Fact(DisplayName = "Should return invalid token when refresh token session is revoked")]
    [Trait("Layer", "Application")]
    [Trait("Category", "UseCases")]
    public async Task Should_Return_InvalidToken_When_RefreshTokenSession_Is_Revoked()
    {
        // Arrange
        var user = CreateActiveUser();
        var dateTimeProvider = new FakeDateTimeProvider();

        var revokedSession = CreateActiveSession(
                user,
                dateTimeProvider.UtcNow)
            .Revoke(dateTimeProvider.UtcNow);

        var refreshTokenStore = new FakeRefreshTokenStore
        {
            Session = revokedSession
        };

        var handler = CreateHandler(
            refreshTokenStore: refreshTokenStore,
            dateTimeProvider: dateTimeProvider);

        var command = new RefreshTokenCommand("current-refresh-token-value");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(RefreshTokenErrors.InvalidToken);

        refreshTokenStore.StoredSession.Should().BeNull();
        refreshTokenStore.DeletedHash.Should().BeNull();
    }

    [Fact(DisplayName = "Should return invalid token when refresh token session is rotated")]
    [Trait("Layer", "Application")]
    [Trait("Category", "UseCases")]
    public async Task Should_Return_InvalidToken_When_RefreshTokenSession_Is_Rotated()
    {
        // Arrange
        var user = CreateActiveUser();
        var dateTimeProvider = new FakeDateTimeProvider();

        var rotatedSession = CreateActiveSession(
                user,
                dateTimeProvider.UtcNow)
            .Rotate(
                Guid.NewGuid(),
                dateTimeProvider.UtcNow);

        var refreshTokenStore = new FakeRefreshTokenStore
        {
            Session = rotatedSession
        };

        var handler = CreateHandler(
            refreshTokenStore: refreshTokenStore,
            dateTimeProvider: dateTimeProvider);

        var command = new RefreshTokenCommand("current-refresh-token-value");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(RefreshTokenErrors.InvalidToken);

        refreshTokenStore.StoredSession.Should().BeNull();
        refreshTokenStore.DeletedHash.Should().BeNull();
    }

    [Fact(DisplayName = "Should return invalid token when user does not exist")]
    [Trait("Layer", "Application")]
    [Trait("Category", "UseCases")]
    public async Task Should_Return_InvalidToken_When_User_Does_Not_Exist()
    {
        // Arrange
        var dateTimeProvider = new FakeDateTimeProvider();

        var currentSession = new RefreshTokenSession(
            SessionId: Guid.NewGuid(),
            UserId: Guid.NewGuid(),
            TenantId: Guid.NewGuid(),
            RefreshTokenHash: "hashed-current-refresh-token-value",
            IssuedAtUtc: dateTimeProvider.UtcNow.AddMinutes(-5),
            ExpiresAtUtc: dateTimeProvider.UtcNow.AddDays(7));

        var userRepository = new FakeUserRepository
        {
            User = null
        };

        var refreshTokenStore = new FakeRefreshTokenStore
        {
            Session = currentSession
        };

        var handler = CreateHandler(
            userRepository: userRepository,
            refreshTokenStore: refreshTokenStore,
            dateTimeProvider: dateTimeProvider);

        var command = new RefreshTokenCommand("current-refresh-token-value");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(RefreshTokenErrors.InvalidToken);

        userRepository.LastUserIdChecked.Should().Be(UserId.From(currentSession.UserId));
        refreshTokenStore.StoredSession.Should().BeNull();
        refreshTokenStore.DeletedHash.Should().BeNull();
    }

    [Fact(DisplayName = "Should return user unavailable when user cannot authenticate")]
    [Trait("Layer", "Application")]
    [Trait("Category", "UseCases")]
    public async Task Should_Return_UserUnavailable_When_User_Cannot_Authenticate()
    {
        // Arrange
        var user = CreateSuspendedUser();
        var dateTimeProvider = new FakeDateTimeProvider();

        var currentSession = CreateActiveSession(
            user,
            dateTimeProvider.UtcNow);

        var refreshTokenStore = new FakeRefreshTokenStore
        {
            Session = currentSession
        };

        var handler = CreateHandler(
            userRepository: new FakeUserRepository { User = user },
            refreshTokenStore: refreshTokenStore,
            dateTimeProvider: dateTimeProvider);

        var command = new RefreshTokenCommand("current-refresh-token-value");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(RefreshTokenErrors.UserUnavailable);

        refreshTokenStore.StoredSession.Should().BeNull();
        refreshTokenStore.DeletedHash.Should().BeNull();
    }

    [Fact(DisplayName = "Should delete old refresh token when refresh succeeds")]
    [Trait("Layer", "Application")]
    [Trait("Category", "UseCases")]
    public async Task Should_Delete_Old_RefreshToken_When_Refresh_Succeeds()
    {
        // Arrange
        var user = CreateActiveUser();
        var dateTimeProvider = new FakeDateTimeProvider();

        var currentSession = CreateActiveSession(
            user,
            dateTimeProvider.UtcNow);

        var refreshTokenStore = new FakeRefreshTokenStore
        {
            Session = currentSession
        };

        var handler = CreateHandler(
            userRepository: new FakeUserRepository { User = user },
            refreshTokenStore: refreshTokenStore,
            dateTimeProvider: dateTimeProvider);

        var command = new RefreshTokenCommand("current-refresh-token-value");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        refreshTokenStore.DeletedHash.Should().Be(currentSession.RefreshTokenHash);
    }

    private static RefreshTokenCommandHandler CreateHandler(
        FakeUserRepository? userRepository = null,
        FakeAccessTokenProvider? accessTokenProvider = null,
        FakeRefreshTokenGenerator? refreshTokenGenerator = null,
        FakeRefreshTokenHasher? refreshTokenHasher = null,
        FakeRefreshTokenStore? refreshTokenStore = null,
        FakeDateTimeProvider? dateTimeProvider = null)
    {
        return new RefreshTokenCommandHandler(
            userRepository ?? new FakeUserRepository(),
            accessTokenProvider ?? new FakeAccessTokenProvider(),
            refreshTokenGenerator ?? new FakeRefreshTokenGenerator(),
            refreshTokenHasher ?? new FakeRefreshTokenHasher(),
            refreshTokenStore ?? new FakeRefreshTokenStore(),
            dateTimeProvider ?? new FakeDateTimeProvider());
    }

    private static RefreshTokenSession CreateActiveSession(
        User user,
        DateTimeOffset utcNow)
    {
        return new RefreshTokenSession(
            SessionId: Guid.NewGuid(),
            UserId: user.Id.Value,
            TenantId: user.TenantId.Value,
            RefreshTokenHash: "hashed-current-refresh-token-value",
            IssuedAtUtc: utcNow.AddMinutes(-5),
            ExpiresAtUtc: utcNow.AddDays(7));
    }

    private static User CreateActiveUser()
    {
        var user = User.Register(
            TenantId.New(),
            "Jeferson Almeida",
            Email.Create("jeferson@gauss.com"),
            PasswordHash.Create("hashed-password"),
            new DateTimeOffset(2026, 04, 30, 12, 0, 0, TimeSpan.Zero));

        user.ConfirmEmail(new DateTimeOffset(2026, 04, 30, 12, 5, 0, TimeSpan.Zero));

        return user;
    }

    private static User CreateSuspendedUser()
    {
        var user = CreateActiveUser();

        user.Suspend();

        return user;
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public User? User { get; init; }

        public UserId? LastUserIdChecked { get; private set; }

        public Task<bool> ExistsByEmailAsync(
            Email email,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        public Task<User?> GetByEmailAsync(
            Email email,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(User);
        }

        public Task<User?> GetByIdAsync(
            UserId userId,
            CancellationToken cancellationToken = default)
        {
            LastUserIdChecked = userId;

            return Task.FromResult(User);
        }

        public Task AddAsync(
            User user,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task UpdateLastLoginAsync(
            User user,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeAccessTokenProvider : IAccessTokenProvider
    {
        public AccessToken AccessToken { get; } = new(
            "new-access-token-value",
            "Bearer",
            new DateTimeOffset(2026, 04, 30, 13, 0, 0, TimeSpan.Zero));

        public User? LastUser { get; private set; }

        public AccessToken Generate(User user)
        {
            LastUser = user;

            return AccessToken;
        }
    }

    private sealed class FakeRefreshTokenGenerator : IRefreshTokenGenerator
    {
        public RefreshToken RefreshToken { get; } = new(
            "new-refresh-token-value",
            new DateTimeOffset(2026, 05, 07, 12, 30, 0, TimeSpan.Zero));

        public DateTimeOffset? LastIssuedAtUtc { get; private set; }

        public RefreshToken Generate(DateTimeOffset issuedAtUtc)
        {
            LastIssuedAtUtc = issuedAtUtc;

            return RefreshToken;
        }
    }

    private sealed class FakeRefreshTokenHasher : IRefreshTokenHasher
    {
        public List<string> HashedRefreshTokens { get; } = [];

        public string Hash(string refreshToken)
        {
            HashedRefreshTokens.Add(refreshToken);

            return refreshToken switch
            {
                "current-refresh-token-value" => "hashed-current-refresh-token-value",
                "missing-refresh-token-value" => "hashed-missing-refresh-token-value",
                "new-refresh-token-value" => "hashed-new-refresh-token-value",
                _ => $"hashed-{refreshToken}"
            };
        }

        public bool Verify(
            string refreshToken,
            string refreshTokenHash)
        {
            return Hash(refreshToken) == refreshTokenHash;
        }
    }

    private sealed class FakeRefreshTokenStore : IRefreshTokenStore
    {
        public RefreshTokenSession? Session { get; init; }

        public string? LastHashChecked { get; private set; }

        public RefreshTokenSession? StoredSession { get; private set; }

        public string? DeletedHash { get; private set; }

        public Task StoreAsync(
            RefreshTokenSession session,
            CancellationToken cancellationToken = default)
        {
            StoredSession = session;

            return Task.CompletedTask;
        }

        public Task<RefreshTokenSession?> GetByHashAsync(
            string refreshTokenHash,
            CancellationToken cancellationToken = default)
        {
            LastHashChecked = refreshTokenHash;

            return Task.FromResult(Session);
        }

        public Task DeleteAsync(
            string refreshTokenHash,
            CancellationToken cancellationToken = default)
        {
            DeletedHash = refreshTokenHash;

            return Task.CompletedTask;
        }
    }

    private sealed class FakeDateTimeProvider : IDateTimeProvider
    {
        public DateTimeOffset UtcNow { get; } =
            new(2026, 04, 30, 12, 30, 0, TimeSpan.Zero);
    }
}
