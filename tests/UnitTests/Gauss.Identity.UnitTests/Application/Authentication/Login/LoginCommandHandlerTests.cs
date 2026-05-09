using AwesomeAssertions;
using Gauss.BuildingBlocks.Domain.Tenants;
using Gauss.Identity.Application.Abstractions.Authentication;
using Gauss.Identity.Application.Abstractions.Persistence;
using Gauss.Identity.Application.Abstractions.Time;
using Gauss.Identity.Application.Authentication.Login;
using Gauss.Identity.Domain.RefreshTokens;
using Gauss.Identity.Domain.Users;
using Gauss.Identity.Domain.Users.ValueObjects;

namespace Gauss.Identity.UnitTests.Application.Authentication.Login;

public sealed class LoginCommandHandlerTests
{
    [Fact(DisplayName = "Should login user when credentials are valid")]
    [Trait("Layer", "Application")]
    [Trait("Category", "UseCases")]
    public async Task Should_Login_User_When_Credentials_Are_Valid()
    {
        // Arrange
        var user = CreateActiveUser();

        var userRepository = new FakeUserRepository
        {
            User = user
        };

        var passwordHasher = new FakePasswordHasher
        {
            VerificationStatus = PasswordVerificationStatus.Success
        };

        var accessTokenProvider = new FakeAccessTokenProvider();
        var refreshTokenGenerator = new FakeRefreshTokenGenerator();
        var refreshTokenHasher = new FakeRefreshTokenHasher();
        var refreshTokenStore = new FakeRefreshTokenStore();
        var dateTimeProvider = new FakeDateTimeProvider();

        var handler = CreateHandler(
            userRepository: userRepository,
            passwordHasher: passwordHasher,
            accessTokenProvider: accessTokenProvider,
            refreshTokenGenerator: refreshTokenGenerator,
            refreshTokenHasher: refreshTokenHasher,
            refreshTokenStore: refreshTokenStore,
            dateTimeProvider: dateTimeProvider);

        var command = new LoginCommand(
            "jeferson@gauss.com",
            "StrongPassword@123");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();

        result.Value.UserId.Should().Be(user.Id.Value);
        result.Value.TenantId.Should().Be(user.TenantId.Value);
        result.Value.Name.Should().Be(user.Name);
        result.Value.Email.Should().Be(user.Email.Value);

        result.Value.AccessToken.Should().Be("access-token-value");
        result.Value.TokenType.Should().Be("Bearer");
        result.Value.ExpiresAtUtc.Should().Be(accessTokenProvider.AccessToken.ExpiresAtUtc);

        result.Value.RefreshToken.Should().Be("refresh-token-value");
        result.Value.RefreshTokenExpiresAtUtc.Should().Be(refreshTokenGenerator.RefreshToken.ExpiresAtUtc);

        userRepository.LastEmailChecked.Should().Be(Email.Create("jeferson@gauss.com"));

        passwordHasher.LastPasswordHash.Should().Be(user.PasswordHash);
        passwordHasher.LastProvidedPassword.Should().Be("StrongPassword@123");

        user.LastLoginAtUtc.Should().Be(dateTimeProvider.UtcNow);
        userRepository.RecordedLoginUserId.Should().Be(user.Id);
        userRepository.RecordedLoginAtUtc.Should().Be(dateTimeProvider.UtcNow);

        accessTokenProvider.LastUser.Should().Be(user);

        refreshTokenGenerator.LastIssuedAtUtc.Should().Be(dateTimeProvider.UtcNow);

        refreshTokenHasher.LastRefreshToken.Should().Be("refresh-token-value");

        refreshTokenStore.StoredSession.Should().NotBeNull();
        refreshTokenStore.StoredSession!.UserId.Should().Be(user.Id.Value);
        refreshTokenStore.StoredSession.TenantId.Should().Be(user.TenantId.Value);
        refreshTokenStore.StoredSession.RefreshTokenHash.Should().Be("hashed-refresh-token-value");
        refreshTokenStore.StoredSession.IssuedAtUtc.Should().Be(dateTimeProvider.UtcNow);
        refreshTokenStore.StoredSession.ExpiresAtUtc.Should().Be(refreshTokenGenerator.RefreshToken.ExpiresAtUtc);
    }

    [Fact(DisplayName = "Should return invalid credentials when user does not exist")]
    [Trait("Layer", "Application")]
    [Trait("Category", "UseCases")]
    public async Task Should_Return_InvalidCredentials_When_User_Does_Not_Exist()
    {
        // Arrange
        var handler = CreateHandler();

        var command = new LoginCommand(
            "missing@gauss.com",
            "StrongPassword@123");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(LoginErrors.InvalidCredentials);
    }

    [Fact(DisplayName = "Should return invalid credentials when password is invalid")]
    [Trait("Layer", "Application")]
    [Trait("Category", "UseCases")]
    public async Task Should_Return_InvalidCredentials_When_Password_Is_Invalid()
    {
        // Arrange
        var user = CreateActiveUser();

        var handler = CreateHandler(
            userRepository: new FakeUserRepository { User = user },
            passwordHasher: new FakePasswordHasher { VerificationStatus = PasswordVerificationStatus.Failed });

        var command = new LoginCommand(
            "jeferson@gauss.com",
            "WrongPassword@123");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(LoginErrors.InvalidCredentials);
    }

    [Fact(DisplayName = "Should return user unavailable when user is not active")]
    [Trait("Layer", "Application")]
    [Trait("Category", "UseCases")]
    public async Task Should_Return_UserUnavailable_When_User_Is_Not_Active()
    {
        // Arrange
        var user = CreatePendingUser();

        var userRepository = new FakeUserRepository
        {
            User = user
        };

        var handler = CreateHandler(
            userRepository: userRepository,
            passwordHasher: new FakePasswordHasher { VerificationStatus = PasswordVerificationStatus.Success });

        var command = new LoginCommand(
            "jeferson@gauss.com",
            "StrongPassword@123");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(LoginErrors.UserUnavailable);
        userRepository.RecordedLoginUserId.Should().BeNull();
        userRepository.RecordedLoginAtUtc.Should().BeNull();
    }

    [Fact(DisplayName = "Should return invalid credentials when email format is invalid")]
    [Trait("Layer", "Application")]
    [Trait("Category", "UseCases")]
    public async Task Should_Return_InvalidCredentials_When_Email_Format_Is_Invalid()
    {
        // Arrange
        var handler = CreateHandler();

        var command = new LoginCommand(
            "invalid-email",
            "StrongPassword@123");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(LoginErrors.InvalidCredentials);
    }

    [Fact(DisplayName = "Should return user unavailable when user is suspended")]
    [Trait("Layer", "Application")]
    [Trait("Category", "UseCases")]
    public async Task Should_Return_UserUnavailable_When_User_Is_Suspended()
    {
        // Arrange
        var suspendedUser = CreateSuspendedUser();

        var userRepository = new FakeUserRepository
        {
            User = suspendedUser
        };

        var handler = CreateHandler(
            userRepository: userRepository,
            passwordHasher: new FakePasswordHasher { VerificationStatus = PasswordVerificationStatus.Success });

        var command = new LoginCommand(
            "jeferson@gauss.com",
            "StrongPassword@123");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(LoginErrors.UserUnavailable);
    }

    [Fact(DisplayName = "Should return user unavailable when user is locked")]
    [Trait("Layer", "Application")]
    [Trait("Category", "UseCases")]
    public async Task Should_Return_UserUnavailable_When_User_Is_Locked()
    {
        // Arrange
        var lockedUser = CreateLockedUser();

        var userRepository = new FakeUserRepository
        {
            User = lockedUser
        };

        var handler = CreateHandler(
            userRepository: userRepository,
            passwordHasher: new FakePasswordHasher { VerificationStatus = PasswordVerificationStatus.Success });

        var command = new LoginCommand(
            "jeferson@gauss.com",
            "StrongPassword@123");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(LoginErrors.UserUnavailable);
    }

    [Fact(DisplayName = "Should return user unavailable when user is deactivated")]
    [Trait("Layer", "Application")]
    [Trait("Category", "UseCases")]
    public async Task Should_Return_UserUnavailable_When_User_Is_Deactivated()
    {
        // Arrange
        var deactivatedUser = CreateDeactivatedUser();

        var userRepository = new FakeUserRepository
        {
            User = deactivatedUser
        };

        var handler = CreateHandler(
            userRepository: userRepository,
            passwordHasher: new FakePasswordHasher { VerificationStatus = PasswordVerificationStatus.Success });

        var command = new LoginCommand(
            "jeferson@gauss.com",
            "StrongPassword@123");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(LoginErrors.UserUnavailable);
    }

    [Fact(DisplayName = "Should return user unavailable when email is not confirmed")]
    [Trait("Layer", "Application")]
    [Trait("Category", "UseCases")]
    public async Task Should_Return_UserUnavailable_When_Email_Is_Not_Confirmed()
    {
        // Arrange
        var user = CreatePendingUser();

        var userRepository = new FakeUserRepository
        {
            User = user
        };

        var handler = CreateHandler(
            userRepository: userRepository,
            passwordHasher: new FakePasswordHasher { VerificationStatus = PasswordVerificationStatus.Success });

        var command = new LoginCommand(
            "jeferson@gauss.com",
            "StrongPassword@123");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(LoginErrors.UserUnavailable);
    }

    [Fact(DisplayName = "Should record LastLoginAtUtc when login is successful")]
    [Trait("Layer", "Application")]
    [Trait("Category", "UseCases")]
    public async Task Should_Record_LastLoginAtUtc_When_Login_Is_Successful()
    {
        // Arrange
        var user = CreateActiveUser();

        var userRepository = new FakeUserRepository
        {
            User = user
        };

        var dateTimeProvider = new FakeDateTimeProvider();

        var handler = CreateHandler(
            userRepository: userRepository,
            passwordHasher: new FakePasswordHasher { VerificationStatus = PasswordVerificationStatus.Success },
            dateTimeProvider: dateTimeProvider);

        var command = new LoginCommand(
            "jeferson@gauss.com",
            "StrongPassword@123");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.LastLoginAtUtc.Should().Be(dateTimeProvider.UtcNow);
        userRepository.RecordedLoginUserId.Should().Be(user.Id);
        userRepository.RecordedLoginAtUtc.Should().Be(dateTimeProvider.UtcNow);
    }

    [Fact(DisplayName = "Should issue refresh token when login is successful")]
    [Trait("Layer", "Application")]
    [Trait("Category", "UseCases")]
    public async Task Should_Issue_RefreshToken_When_Login_Is_Successful()
    {
        // Arrange
        var user = CreateActiveUser();

        var refreshTokenGenerator = new FakeRefreshTokenGenerator();
        var refreshTokenHasher = new FakeRefreshTokenHasher();
        var refreshTokenStore = new FakeRefreshTokenStore();
        var dateTimeProvider = new FakeDateTimeProvider();

        var handler = CreateHandler(
            userRepository: new FakeUserRepository { User = user },
            passwordHasher: new FakePasswordHasher { VerificationStatus = PasswordVerificationStatus.Success },
            refreshTokenGenerator: refreshTokenGenerator,
            refreshTokenHasher: refreshTokenHasher,
            refreshTokenStore: refreshTokenStore,
            dateTimeProvider: dateTimeProvider);

        var command = new LoginCommand(
            "jeferson@gauss.com",
            "StrongPassword@123");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();

        result.Value.RefreshToken.Should().Be(refreshTokenGenerator.RefreshToken.Value);
        result.Value.RefreshTokenExpiresAtUtc.Should().Be(refreshTokenGenerator.RefreshToken.ExpiresAtUtc);

        refreshTokenGenerator.LastIssuedAtUtc.Should().Be(dateTimeProvider.UtcNow);
        refreshTokenHasher.LastRefreshToken.Should().Be(refreshTokenGenerator.RefreshToken.Value);

        refreshTokenStore.StoredSession.Should().NotBeNull();
        refreshTokenStore.StoredSession!.UserId.Should().Be(user.Id.Value);
        refreshTokenStore.StoredSession.TenantId.Should().Be(user.TenantId.Value);
        refreshTokenStore.StoredSession.RefreshTokenHash.Should().Be("hashed-refresh-token-value");
        refreshTokenStore.StoredSession.IssuedAtUtc.Should().Be(dateTimeProvider.UtcNow);
        refreshTokenStore.StoredSession.ExpiresAtUtc.Should().Be(refreshTokenGenerator.RefreshToken.ExpiresAtUtc);
    }

    [Fact(DisplayName = "Should not issue refresh token when password is invalid")]
    [Trait("Layer", "Application")]
    [Trait("Category", "UseCases")]
    public async Task Should_Not_Issue_RefreshToken_When_Password_Is_Invalid()
    {
        // Arrange
        var user = CreateActiveUser();

        var refreshTokenGenerator = new FakeRefreshTokenGenerator();
        var refreshTokenHasher = new FakeRefreshTokenHasher();
        var refreshTokenStore = new FakeRefreshTokenStore();

        var handler = CreateHandler(
            userRepository: new FakeUserRepository { User = user },
            passwordHasher: new FakePasswordHasher { VerificationStatus = PasswordVerificationStatus.Failed },
            refreshTokenGenerator: refreshTokenGenerator,
            refreshTokenHasher: refreshTokenHasher,
            refreshTokenStore: refreshTokenStore);

        var command = new LoginCommand(
            "jeferson@gauss.com",
            "WrongPassword@123");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(LoginErrors.InvalidCredentials);

        refreshTokenGenerator.LastIssuedAtUtc.Should().BeNull();
        refreshTokenHasher.LastRefreshToken.Should().BeNull();
        refreshTokenStore.StoredSession.Should().BeNull();
    }

    [Fact(DisplayName = "Should not issue refresh token when user is unavailable")]
    [Trait("Layer", "Application")]
    [Trait("Category", "UseCases")]
    public async Task Should_Not_Issue_RefreshToken_When_User_Is_Unavailable()
    {
        // Arrange
        var user = CreatePendingUser();

        var refreshTokenGenerator = new FakeRefreshTokenGenerator();
        var refreshTokenHasher = new FakeRefreshTokenHasher();
        var refreshTokenStore = new FakeRefreshTokenStore();

        var handler = CreateHandler(
            userRepository: new FakeUserRepository { User = user },
            passwordHasher: new FakePasswordHasher { VerificationStatus = PasswordVerificationStatus.Success },
            refreshTokenGenerator: refreshTokenGenerator,
            refreshTokenHasher: refreshTokenHasher,
            refreshTokenStore: refreshTokenStore);

        var command = new LoginCommand(
            "jeferson@gauss.com",
            "StrongPassword@123");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(LoginErrors.UserUnavailable);

        refreshTokenGenerator.LastIssuedAtUtc.Should().BeNull();
        refreshTokenHasher.LastRefreshToken.Should().BeNull();
        refreshTokenStore.StoredSession.Should().BeNull();
    }

    private static LoginCommandHandler CreateHandler(
        FakeUserRepository? userRepository = null,
        FakePasswordHasher? passwordHasher = null,
        FakeAccessTokenProvider? accessTokenProvider = null,
        FakeRefreshTokenGenerator? refreshTokenGenerator = null,
        FakeRefreshTokenHasher? refreshTokenHasher = null,
        FakeRefreshTokenStore? refreshTokenStore = null,
        FakeDateTimeProvider? dateTimeProvider = null)
    {
        return new LoginCommandHandler(
            userRepository ?? new FakeUserRepository(),
            passwordHasher ?? new FakePasswordHasher(),
            accessTokenProvider ?? new FakeAccessTokenProvider(),
            refreshTokenGenerator ?? new FakeRefreshTokenGenerator(),
            refreshTokenHasher ?? new FakeRefreshTokenHasher(),
            refreshTokenStore ?? new FakeRefreshTokenStore(),
            dateTimeProvider ?? new FakeDateTimeProvider());
    }

    private static User CreateDeactivatedUser()
    {
        var user = User.Register(
            TenantId.New(),
            "Jeferson Almeida",
            Email.Create("jeferson@gauss.com"),
            PasswordHash.Create("hashed-password"),
            new DateTimeOffset(2026, 04, 30, 12, 0, 0, TimeSpan.Zero));

        user.Deactivate();

        return user;
    }

    private static User CreateLockedUser()
    {
        var user = User.Register(
            TenantId.New(),
            "Jeferson Almeida",
            Email.Create("jeferson@gauss.com"),
            PasswordHash.Create("hashed-password"),
            new DateTimeOffset(2026, 04, 30, 12, 0, 0, TimeSpan.Zero));

        var utcNow = new DateTimeOffset(2026, 04, 30, 12, 0, 0, TimeSpan.Zero);
        user.LockUntil(utcNow.AddHours(1), utcNow);

        return user;
    }

    private static User CreateSuspendedUser()
    {
        var user = User.Register(
            TenantId.New(),
            "Jeferson Almeida",
            Email.Create("jeferson@gauss.com"),
            PasswordHash.Create("hashed-password"),
            new DateTimeOffset(2026, 04, 30, 12, 0, 0, TimeSpan.Zero));

        user.Suspend();

        return user;
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

    private static User CreatePendingUser()
    {
        return User.Register(
            TenantId.New(),
            "Jeferson Almeida",
            Email.Create("jeferson@gauss.com"),
            PasswordHash.Create("hashed-password"),
            new DateTimeOffset(2026, 04, 30, 12, 0, 0, TimeSpan.Zero));
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public bool EmailAlreadyExists { get; init; }

        public User? User { get; init; }

        public Email? LastEmailChecked { get; private set; }

        public User? AddedUser { get; private set; }

        public UserId? RecordedLoginUserId { get; private set; }

        public DateTimeOffset? RecordedLoginAtUtc { get; private set; }

        public Task<bool> ExistsByEmailAsync(
            Email email,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(EmailAlreadyExists);
        }

        public Task<User?> GetByEmailAsync(
            Email email,
            CancellationToken cancellationToken = default)
        {
            LastEmailChecked = email;

            return Task.FromResult(User);
        }

        public Task AddAsync(
            User user,
            CancellationToken cancellationToken = default)
        {
            AddedUser = user;

            return Task.CompletedTask;
        }

        public Task RecordLoginAsync(
            UserId userId,
            DateTimeOffset loggedInAtUtc,
            CancellationToken cancellationToken = default)
        {
            RecordedLoginUserId = userId;
            RecordedLoginAtUtc = loggedInAtUtc;

            return Task.CompletedTask;
        }

        public Task<User?> GetByIdAsync(
            UserId userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(User);
        }
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public PasswordVerificationStatus VerificationStatus { get; init; } =
            PasswordVerificationStatus.Success;

        public PasswordHash? LastPasswordHash { get; private set; }

        public string? LastProvidedPassword { get; private set; }

        public PasswordHash Hash(string password)
        {
            return PasswordHash.Create($"hashed-{password}");
        }

        public PasswordVerificationStatus Verify(
            PasswordHash passwordHash,
            string providedPassword)
        {
            LastPasswordHash = passwordHash;
            LastProvidedPassword = providedPassword;

            return VerificationStatus;
        }
    }

    private sealed class FakeAccessTokenProvider : IAccessTokenProvider
    {
        public AccessToken AccessToken { get; } = new(
            "access-token-value",
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
            "refresh-token-value",
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
        public string? LastRefreshToken { get; private set; }

        public string Hash(string refreshToken)
        {
            LastRefreshToken = refreshToken;

            return "hashed-refresh-token-value";
        }

        public bool Verify(
            string refreshToken,
            string refreshTokenHash)
        {
            return refreshToken == "refresh-token-value"
                && refreshTokenHash == "hashed-refresh-token-value";
        }
    }

    private sealed class FakeRefreshTokenStore : IRefreshTokenStore
    {
        public RefreshTokenSession? StoredSession { get; private set; }

        public RefreshTokenSession? UpdatedSession { get; private set; }

        public Guid? RevokedFamilyId { get; private set; }

        public DateTimeOffset? RevokedAtUtc { get; private set; }

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
            return Task.FromResult<RefreshTokenSession?>(StoredSession);
        }

        public Task UpdateAsync(
            RefreshTokenSession session,
            CancellationToken cancellationToken = default)
        {
            UpdatedSession = session;
            StoredSession = session;

            return Task.CompletedTask;
        }

        public Task RevokeFamilyAsync(
            Guid familyId,
            DateTimeOffset revokedAtUtc,
            CancellationToken cancellationToken = default)
        {
            RevokedFamilyId = familyId;
            RevokedAtUtc = revokedAtUtc;

            if (StoredSession is not null && StoredSession.FamilyId == familyId)
            {
                StoredSession = StoredSession.Revoke(revokedAtUtc);
            }

            if (UpdatedSession is not null && UpdatedSession.FamilyId == familyId)
            {
                UpdatedSession = UpdatedSession.Revoke(revokedAtUtc);
            }

            return Task.CompletedTask;
        }
    }

    private sealed class FakeDateTimeProvider : IDateTimeProvider
    {
        public DateTimeOffset UtcNow { get; } =
            new(2026, 04, 30, 12, 30, 0, TimeSpan.Zero);
    }
}
