using AwesomeAssertions;
using Gauss.Identity.Application.Abstractions.Authentication;
using Gauss.Identity.Application.Abstractions.Persistence;
using Gauss.Identity.Application.Abstractions.Time;
using Gauss.Identity.Application.Users.RegisterUser;
using Gauss.Identity.Domain.Users;
using Gauss.Identity.Domain.Users.ValueObjects;

namespace Gauss.Identity.UnitTests.Application.Users.RegisterUser;

public sealed class RegisterUserCommandHandlerTests
{
    [Fact(DisplayName = "Should register user when command is valid")]
    [Trait("Layer", "Application")]
    [Trait("Category", "UseCases")]
    public async Task Should_Register_User_When_Command_Is_Valid()
    {
        // Arrange
        var userRepository = new FakeUserRepository();
        var passwordHasher = new FakePasswordHasher();
        var dateTimeProvider = new FakeDateTimeProvider();

        var handler = new RegisterUserCommandHandler(
            userRepository,
            passwordHasher,
            dateTimeProvider);

        var command = new RegisterUserCommand(
            "Jeferson Almeida",
            "jeferson@gauss.com",
            "StrongPassword@123");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().NotBe(Guid.Empty);
        result.Value.TenantId.Should().NotBe(Guid.Empty);
        result.Value.Name.Should().Be("Jeferson Almeida");
        result.Value.Email.Should().Be("jeferson@gauss.com");

        userRepository.AddedUser.Should().NotBeNull();
        userRepository.AddedUser!.Email.Value.Should().Be("jeferson@gauss.com");
        userRepository.AddedUser.PasswordHash.Value.Should().Be("hashed-StrongPassword@123");
        userRepository.AddedUser.RegisteredAtUtc.Should().Be(dateTimeProvider.UtcNow);

        passwordHasher.LastPassword.Should().Be("StrongPassword@123");
    }
    
    [Theory(DisplayName = "Should return invalid email error when email is invalid")]
    [Trait("Layer", "Application")]
    [Trait("Category", "UseCases")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid-email")]
    [InlineData("user@")]
    [InlineData("@gauss.com")]
    public async Task Should_Return_InvalidEmail_Error_When_Email_Is_Invalid(string email)
    {
        // Arrange
        var handler = CreateHandler();

        var command = new RegisterUserCommand(
            "Jeferson Almeida",
            email,
            "StrongPassword@123");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(RegisterUserErrors.InvalidEmail);
    }

    [Fact(DisplayName = "Should return email already exists error when email is duplicated")]
    [Trait("Layer", "Application")]
    [Trait("Category", "UseCases")]
    public async Task Should_Return_EmailAlreadyExists_Error_When_Email_Is_Duplicated()
    {
        // Arrange
        var userRepository = new FakeUserRepository
        {
            EmailAlreadyExists = true
        };

        var handler = new RegisterUserCommandHandler(
            userRepository,
            new FakePasswordHasher(),
            new FakeDateTimeProvider());

        var command = new RegisterUserCommand(
            "Jeferson Almeida",
            "jeferson@gauss.com",
            "StrongPassword@123");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(RegisterUserErrors.EmailAlreadyExists);
        userRepository.AddedUser.Should().BeNull();
    }

    private static RegisterUserCommandHandler CreateHandler()
    {
        return new RegisterUserCommandHandler(
            new FakeUserRepository(),
            new FakePasswordHasher(),
            new FakeDateTimeProvider());
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public bool EmailAlreadyExists { get; init; }

        public User? AddedUser { get; private set; }

        public Task<bool> ExistsByEmailAsync(
            Email email,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(EmailAlreadyExists);
        }

        public Task AddAsync(
            User user,
            CancellationToken cancellationToken = default)
        {
            AddedUser = user;

            return Task.CompletedTask;
        }
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string? LastPassword { get; private set; }

        public PasswordHash Hash(string password)
        {
            LastPassword = password;

            return PasswordHash.Create($"hashed-{password}");
        }
    }

    private sealed class FakeDateTimeProvider : IDateTimeProvider
    {
        public DateTimeOffset UtcNow { get; } =
            new(2026, 04, 30, 12, 0, 0, TimeSpan.Zero);
    }
}
