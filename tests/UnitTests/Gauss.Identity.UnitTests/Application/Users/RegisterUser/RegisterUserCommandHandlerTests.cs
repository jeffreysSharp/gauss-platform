using AwesomeAssertions;
using Gauss.BuildingBlocks.Domain.Tenants;
using Gauss.Identity.Application.Abstractions.Authentication;
using Gauss.Identity.Application.Abstractions.Persistence;
using Gauss.Identity.Application.Abstractions.Provisioning;
using Gauss.Identity.Application.Abstractions.Time;
using Gauss.Identity.Application.Users.RegisterUser;
using Gauss.Identity.Domain.Roles;
using Gauss.Identity.Domain.Roles.ValueObjects;
using Gauss.Identity.Domain.Users;
using Gauss.Identity.Domain.Users.ValueObjects;

namespace Gauss.Identity.UnitTests.Application.Users.RegisterUser;

public sealed class RegisterUserCommandHandlerTests
{
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

    [Fact(DisplayName = "Should not call provisioning when email input is invalid")]
    [Trait("Layer", "Application")]
    [Trait("Category", "UseCases")]
    public async Task Should_Not_Call_Provisioning_When_Email_Input_Is_Invalid()
    {
        // Arrange
        var provisioningService = new FakeRegistrationProvisioningService();
        var handler = CreateHandler(registrationProvisioningService: provisioningService);

        var command = new RegisterUserCommand(
            "Jeferson Almeida",
            "invalid-email",
            "StrongPassword@123");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        provisioningService.WasCalled.Should().BeFalse();
    }

    [Fact(DisplayName = "Should return email already exists error when email is duplicated")]
    [Trait("Layer", "Application")]
    [Trait("Category", "UseCases")]
    public async Task Should_Return_EmailAlreadyExists_Error_When_Email_Is_Duplicated()
    {
        // Arrange
        var userRepository = new FakeUserRepository { EmailAlreadyExists = true };
        var provisioningService = new FakeRegistrationProvisioningService();
        var handler = CreateHandler(userRepository: userRepository, registrationProvisioningService: provisioningService);

        var command = new RegisterUserCommand(
            "Jeferson Almeida",
            "jeferson@gauss.com",
            "StrongPassword@123");

        // Act
        var result = await handler.HandleAsync(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(RegisterUserErrors.EmailAlreadyExists);
        provisioningService.WasCalled.Should().BeFalse();
    }

    [Fact(DisplayName = "Should register user when command is valid")]
    [Trait("Layer", "Application")]
    [Trait("Category", "UseCases")]
    public async Task Should_Register_User_When_Command_Is_Valid()
    {
        // Arrange
        var userRepository = new FakeUserRepository();
        var passwordHasher = new FakePasswordHasher();
        var dateTimeProvider = new FakeDateTimeProvider();
        var permissionRepository = new FakePermissionRepository();
        var provisioningService = new FakeRegistrationProvisioningService();

        var handler = new RegisterUserCommandHandler(
            userRepository,
            permissionRepository,
            provisioningService,
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

        provisioningService.WasCalled.Should().BeTrue();
        provisioningService.ProvisionedUser.Should().NotBeNull();
        provisioningService.ProvisionedUser!.Email.Value.Should().Be("jeferson@gauss.com");
        provisioningService.ProvisionedRole.Should().NotBeNull();
        provisioningService.ProvisionedRole!.Name.Should().Be(RoleName.Create("Tenant Administrator"));
        provisioningService.ProvisionedUserRole.Should().NotBeNull();
        provisioningService.ProvisionedUserRole!.UserId.Should().Be(provisioningService.ProvisionedUser.Id);
        provisioningService.ProvisionedUserRole.RoleId.Should().Be(provisioningService.ProvisionedRole.Id);
        provisioningService.ProvisionedRole!.Permissions.Should().NotBeEmpty();
        passwordHasher.LastPassword.Should().Be("StrongPassword@123");
    }

    private static RegisterUserCommandHandler CreateHandler(
        FakeUserRepository? userRepository = null,
        FakePermissionRepository? permissionRepository = null,
        FakeRegistrationProvisioningService? registrationProvisioningService = null,
        FakePasswordHasher? passwordHasher = null,
        FakeDateTimeProvider? dateTimeProvider = null)
    {
        return new RegisterUserCommandHandler(
            userRepository ?? new FakeUserRepository(),
            permissionRepository ?? new FakePermissionRepository(),
            registrationProvisioningService ?? new FakeRegistrationProvisioningService(),
            passwordHasher ?? new FakePasswordHasher(),
            dateTimeProvider ?? new FakeDateTimeProvider());
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public bool EmailAlreadyExists { get; init; }

        public User? User { get; init; }

        public Email? LastEmailChecked { get; private set; }

        public User? AddedUser { get; private set; }

        public Task UpdatePasswordHashAsync(
            UserId userId,
            PasswordHash passwordHash,
            DateTimeOffset updatedAtUtc,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
        public Task<bool> ExistsByEmailAsync(
            Email email,
            CancellationToken cancellationToken = default)
        {
            LastEmailChecked = email;

            return Task.FromResult(EmailAlreadyExists);
        }

        public Task<User?> GetByEmailAsync(
            Email email,
            CancellationToken cancellationToken = default)
        {
            LastEmailChecked = email;

            return Task.FromResult(User);
        }

        public Task<User?> GetByIdAsync(
            UserId userId,
            CancellationToken cancellationToken = default)
        {
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

        public PasswordVerificationStatus Verify(PasswordHash passwordHash, string providedPassword)
            => passwordHash.Value == $"hashed-{providedPassword}"
                ? PasswordVerificationStatus.Success
                : PasswordVerificationStatus.Failed;
    }

    private sealed class FakePermissionRepository : IPermissionRepository
    {
        public Task<bool> ExistsByCodeAsync(PermissionCode code, CancellationToken cancellationToken = default)
            => Task.FromResult(true);

        public Task<Permission?> GetByCodeAsync(PermissionCode code, CancellationToken cancellationToken = default)
        {
            var permission = Permission.Create(code, $"Permission {code.Value}.", new DateTimeOffset(2026, 04, 30, 12, 0, 0, TimeSpan.Zero));
            return Task.FromResult<Permission?>(permission);
        }

        public Task<IReadOnlyCollection<Permission>> GetAllEnabledAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyCollection<Permission>>(Array.Empty<Permission>());

        public Task AddAsync(Permission permission, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task UpdateAsync(Permission permission, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeRegistrationProvisioningService : IRegistrationProvisioningService
    {
        public bool WasCalled { get; private set; }
        public TenantId? ProvisionedTenantId { get; private set; }
        public string? ProvisionedTenantName { get; private set; }
        public User? ProvisionedUser { get; private set; }
        public Role? ProvisionedRole { get; private set; }
        public UserRole? ProvisionedUserRole { get; private set; }

        public Task ProvisionAsync(
            TenantId tenantId,
            string tenantName,
            User user,
            Role adminRole,
            UserRole userRole,
            CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            ProvisionedTenantId = tenantId;
            ProvisionedTenantName = tenantName;
            ProvisionedUser = user;
            ProvisionedRole = adminRole;
            ProvisionedUserRole = userRole;

            return Task.CompletedTask;
        }
    }

    private sealed class FakeDateTimeProvider : IDateTimeProvider
    {
        public DateTimeOffset UtcNow { get; } = new(2026, 04, 30, 12, 0, 0, TimeSpan.Zero);
    }
}
