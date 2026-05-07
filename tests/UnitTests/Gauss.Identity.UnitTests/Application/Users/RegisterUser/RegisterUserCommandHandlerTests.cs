using AwesomeAssertions;
using Gauss.Identity.Application.Abstractions.Authentication;
using Gauss.Identity.Application.Abstractions.Persistence;
using Gauss.Identity.Application.Abstractions.Time;
using Gauss.Identity.Application.Users.RegisterUser;
using Gauss.Identity.Domain.Roles;
using Gauss.Identity.Domain.Roles.ValueObjects;
using Gauss.Identity.Domain.Tenants;
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
        var permissionRepository = new FakePermissionRepository();
        var roleRepository = new FakeRoleRepository();

        var handler = new RegisterUserCommandHandler(
            userRepository,
            permissionRepository,
            roleRepository,
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
        userRepository.LastEmailChecked.Should().Be(Email.Create("jeferson@gauss.com"));
        passwordHasher.LastPassword.Should().Be("StrongPassword@123");

        roleRepository.AddedRole.Should().NotBeNull();
        roleRepository.AddedRole!.TenantId.Should().Be(userRepository.AddedUser!.TenantId);
        roleRepository.AddedRole.Name.Should().Be(RoleName.Create("Tenant Administrator"));
        roleRepository.AddedRole.Permissions.Should().HaveCount(7);

        roleRepository.AssignedUserRole.Should().NotBeNull();
        roleRepository.AssignedUserRole!.UserId.Should().Be(userRepository.AddedUser.Id);
        roleRepository.AssignedUserRole.TenantId.Should().Be(userRepository.AddedUser.TenantId);
        roleRepository.AssignedUserRole.RoleId.Should().Be(roleRepository.AddedRole.Id);
        roleRepository.AssignedUserRole.AssignedAtUtc.Should().Be(dateTimeProvider.UtcNow);
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

        var passwordHasher = new FakePasswordHasher();
        var dateTimeProvider = new FakeDateTimeProvider();
        var permissionRepository = new FakePermissionRepository();
        var roleRepository = new FakeRoleRepository();

        var handler = new RegisterUserCommandHandler(
            userRepository,
            permissionRepository,
            roleRepository,
            passwordHasher,
            dateTimeProvider);

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

        roleRepository.AddedRole.Should().BeNull();
        roleRepository.AssignedUserRole.Should().BeNull();

        passwordHasher.LastPassword.Should().BeNull();
    }

    private static RegisterUserCommandHandler CreateHandler(
        FakeUserRepository? userRepository = null,
        FakePermissionRepository? permissionRepository = null,
        FakeRoleRepository? roleRepository = null,
        FakePasswordHasher? passwordHasher = null,
        FakeDateTimeProvider? dateTimeProvider = null)
    {
        return new RegisterUserCommandHandler(
            userRepository ?? new FakeUserRepository(),
            permissionRepository ?? new FakePermissionRepository(),
            roleRepository ?? new FakeRoleRepository(),
            passwordHasher ?? new FakePasswordHasher(),
            dateTimeProvider ?? new FakeDateTimeProvider());
    }

    private sealed class FakePermissionRepository : IPermissionRepository
    {
        public Task<bool> ExistsByCodeAsync(
            PermissionCode code,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        public Task<Permission?> GetByCodeAsync(
            PermissionCode code,
            CancellationToken cancellationToken = default)
        {
            var permission = Permission.Create(
                code,
                $"Permission {code.Value}.",
                new DateTimeOffset(2026, 04, 30, 12, 0, 0, TimeSpan.Zero));

            return Task.FromResult<Permission?>(permission);
        }

        public Task<IReadOnlyCollection<Permission>> GetAllEnabledAsync(
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<Permission>>([]);
        }

        public Task AddAsync(
            Permission permission,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task UpdateAsync(
            Permission permission,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeRoleRepository : IRoleRepository
    {
        public Role? AddedRole { get; private set; }

        public UserRole? AssignedUserRole { get; private set; }

        public Task<bool> ExistsByNameAsync(
            TenantId tenantId,
            RoleName name,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        public Task<Role?> GetByIdAsync(
            RoleId roleId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Role?>(AddedRole);
        }

        public Task<IReadOnlyCollection<Role>> GetByUserAsync(
            TenantId tenantId,
            UserId userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IReadOnlyCollection<Role>>(
                AddedRole is null ? [] : [AddedRole]);
        }

        public Task AddAsync(
            Role role,
            CancellationToken cancellationToken = default)
        {
            AddedRole = role;

            return Task.CompletedTask;
        }

        public Task UpdateAsync(
            Role role,
            CancellationToken cancellationToken = default)
        {
            AddedRole = role;

            return Task.CompletedTask;
        }

        public Task AssignToUserAsync(
            UserRole userRole,
            CancellationToken cancellationToken = default)
        {
            AssignedUserRole = userRole;

            return Task.CompletedTask;
        }
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public bool EmailAlreadyExists { get; init; }

        public Email? LastEmailChecked { get; private set; }

        public User? AddedUser { get; private set; }

        public Task<bool> ExistsByEmailAsync(
            Email email,
            CancellationToken cancellationToken = default)
        {
            LastEmailChecked = email;

            return Task.FromResult(EmailAlreadyExists);
        }

        public Task AddAsync(
            User user,
            CancellationToken cancellationToken = default)
        {
            AddedUser = user;

            return Task.CompletedTask;
        }

        public Task<User?> GetByEmailAsync(
            Email email,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<User?>(null);
        }

        public Task UpdateLastLoginAsync(
            User user,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task<User?> GetByIdAsync(
            UserId userId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<User?>(null);
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

        public PasswordVerificationStatus Verify(
            PasswordHash passwordHash,
            string providedPassword)
        {
            return passwordHash.Value == $"hashed-{providedPassword}"
                ? PasswordVerificationStatus.Success
                : PasswordVerificationStatus.Failed;
        }
    }

    private sealed class FakeDateTimeProvider : IDateTimeProvider
    {
        public DateTimeOffset UtcNow { get; } =
            new(2026, 04, 30, 12, 0, 0, TimeSpan.Zero);
    }
}
