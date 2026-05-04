using AwesomeAssertions;
using Gauss.Identity.Application.Abstractions.Authentication;
using Gauss.Identity.Application.Abstractions.Persistence;
using Gauss.Identity.Application.Abstractions.Time;
using Gauss.Identity.Application.Authentication.Login;
using Gauss.Identity.Domain.Users;
using Gauss.Identity.Domain.Users.Tenancy;
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
        var dateTimeProvider = new FakeDateTimeProvider();

        var handler = new LoginCommandHandler(
            userRepository,
            passwordHasher,
            accessTokenProvider,
            dateTimeProvider);

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

        userRepository.LastEmailChecked.Should().Be(Email.Create("jeferson@gauss.com"));
        passwordHasher.LastPasswordHash.Should().Be(user.PasswordHash);
        passwordHasher.LastProvidedPassword.Should().Be("StrongPassword@123");
        userRepository.UpdatedUser.Should().Be(user);
        user.LastLoginAtUtc.Should().Be(dateTimeProvider.UtcNow);
        accessTokenProvider.LastUser.Should().Be(user);
    }

    [Fact(DisplayName = "Should return invalid credentials when user does not exist")]
    [Trait("Layer", "Application")]
    [Trait("Category", "UseCases")]
    public async Task Should_Return_InvalidCredentials_When_User_Does_Not_Exist()
    {
        // Arrange
        var handler = new LoginCommandHandler(
            new FakeUserRepository(),
            new FakePasswordHasher(),
            new FakeAccessTokenProvider(),
            new FakeDateTimeProvider());

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

        var handler = new LoginCommandHandler(
            new FakeUserRepository { User = user },
            new FakePasswordHasher { VerificationStatus = PasswordVerificationStatus.Failed },
            new FakeAccessTokenProvider(),
            new FakeDateTimeProvider());

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

        var handler = new LoginCommandHandler(
            userRepository,
            new FakePasswordHasher { VerificationStatus = PasswordVerificationStatus.Success },
            new FakeAccessTokenProvider(),
            new FakeDateTimeProvider());

        var command = new LoginCommand(
            "jeferson@gauss.com",
            "StrongPassword@123");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(LoginErrors.UserUnavailable);
        userRepository.UpdatedUser.Should().BeNull();
    }

    [Fact(DisplayName = "Should return invalid email when email format is invalid")]
    [Trait("Layer", "Application")]
    [Trait("Category", "UseCases")]
    public async Task Should_Return_InvalidEmail_When_Email_Format_Is_Invalid()
    {
        // Arrange
        var handler = new LoginCommandHandler(
            new FakeUserRepository(),
            new FakePasswordHasher(),
            new FakeAccessTokenProvider(),
            new FakeDateTimeProvider());

        var command = new LoginCommand(
            "invalid-email",
            "StrongPassword@123");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(LoginErrors.InvalidEmail);
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

        var handler = new LoginCommandHandler(
            userRepository,
            new FakePasswordHasher { VerificationStatus = PasswordVerificationStatus.Success },
            new FakeAccessTokenProvider(),
            new FakeDateTimeProvider());

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

        var handler = new LoginCommandHandler(
            userRepository,
            new FakePasswordHasher { VerificationStatus = PasswordVerificationStatus.Success },
            new FakeAccessTokenProvider(),
            new FakeDateTimeProvider());

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

        var handler = new LoginCommandHandler(
            userRepository,
            new FakePasswordHasher { VerificationStatus = PasswordVerificationStatus.Success },
            new FakeAccessTokenProvider(),
            new FakeDateTimeProvider());

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

        var handler = new LoginCommandHandler(
            userRepository,
            new FakePasswordHasher { VerificationStatus = PasswordVerificationStatus.Success },
            new FakeAccessTokenProvider(),
            new FakeDateTimeProvider());

        var command = new LoginCommand(
            "jeferson@gauss.com",
            "StrongPassword@123");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(LoginErrors.UserUnavailable);
    }

    [Fact(DisplayName = "Should update LastLoginAtUtc when login is successful")]
    [Trait("Layer", "Application")]
    [Trait("Category", "UseCases")]
    public async Task Should_Update_LastLoginAtUtc_When_Login_Is_Successful()
    {
        // Arrange
        var user = CreateActiveUser();

        var userRepository = new FakeUserRepository
        {
            User = user
        };

        var handler = new LoginCommandHandler(
            userRepository,
            new FakePasswordHasher { VerificationStatus = PasswordVerificationStatus.Success },
            new FakeAccessTokenProvider(),
            new FakeDateTimeProvider());

        var command = new LoginCommand(
            "jeferson@gauss.com",
            "StrongPassword@123");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        user.LastLoginAtUtc.Should().NotBeNull();
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

        user.LockUntil(DateTimeOffset.UtcNow.AddHours(1));

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

        public User? UpdatedUser { get; private set; }

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

        public Task UpdateLastLoginAsync(
            User user,
            CancellationToken cancellationToken = default)
        {
            UpdatedUser = user;

            return Task.CompletedTask;
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

    private sealed class FakeDateTimeProvider : IDateTimeProvider
    {
        public DateTimeOffset UtcNow { get; } =
            new(2026, 04, 30, 12, 30, 0, TimeSpan.Zero);
    }
}
