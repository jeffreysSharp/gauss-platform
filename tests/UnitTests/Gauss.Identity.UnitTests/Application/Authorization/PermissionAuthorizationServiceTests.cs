using AwesomeAssertions;
using Gauss.Identity.Application.Abstractions.Authentication;
using Gauss.Identity.Application.Abstractions.Persistence;
using Gauss.Identity.Application.Abstractions.Tenancy;
using Gauss.Identity.Application.Authorization;
using Gauss.Identity.Domain.Roles;
using Gauss.Identity.Domain.Roles.ValueObjects;
using Gauss.Identity.Domain.Tenants;
using Gauss.Identity.Domain.Users;

namespace Gauss.Identity.UnitTests.Application.Authorization;

public sealed class PermissionAuthorizationServiceTests
{
    [Fact(DisplayName = "Should return true when authenticated user has permission in current tenant")]
    [Trait("Layer", "Application")]
    [Trait("Category", "Authorization")]
    public async Task Should_Return_True_When_Authenticated_User_Has_Permission_In_Current_Tenant()
    {
        // Arrange
        var userId = UserId.New();
        var tenantId = TenantId.New();

        var permission = CreatePermission("Identity.Users.Read");

        var role = Role.Create(
            tenantId,
            RoleName.Create("Admin"),
            new DateTimeOffset(2026, 05, 07, 12, 0, 0, TimeSpan.Zero));

        role.GrantPermission(permission);

        var roleRepository = new FakeRoleRepository
        {
            Roles = [role]
        };

        var service = CreateService(
            currentUserContext: new FakeCurrentUserContext
            {
                IsAuthenticated = true,
                UserId = userId.Value,
                TenantId = tenantId.Value
            },
            currentTenantContext: new FakeCurrentTenantContext
            {
                CurrentTenantId = tenantId
            },
            roleRepository: roleRepository);

        // Act
        var hasPermission = await service.HasPermissionAsync(permission.Code);

        // Assert
        hasPermission.Should().BeTrue();
        roleRepository.LastTenantId.Should().Be(tenantId);
        roleRepository.LastUserId.Should().Be(userId);
    }

    [Fact(DisplayName = "Should return false when user is not authenticated")]
    [Trait("Layer", "Application")]
    [Trait("Category", "Authorization")]
    public async Task Should_Return_False_When_User_Is_Not_Authenticated()
    {
        // Arrange
        var service = CreateService(
            currentUserContext: new FakeCurrentUserContext
            {
                IsAuthenticated = false
            });

        // Act
        var hasPermission = await service.HasPermissionAsync(
            PermissionCode.Create("Identity.Users.Read"));

        // Assert
        hasPermission.Should().BeFalse();
    }

    [Fact(DisplayName = "Should return false when user id is missing")]
    [Trait("Layer", "Application")]
    [Trait("Category", "Authorization")]
    public async Task Should_Return_False_When_UserId_Is_Missing()
    {
        // Arrange
        var tenantId = TenantId.New();

        var service = CreateService(
            currentUserContext: new FakeCurrentUserContext
            {
                IsAuthenticated = true,
                UserId = null,
                TenantId = tenantId.Value
            },
            currentTenantContext: new FakeCurrentTenantContext
            {
                CurrentTenantId = tenantId
            });

        // Act
        var hasPermission = await service.HasPermissionAsync(
            PermissionCode.Create("Identity.Users.Read"));

        // Assert
        hasPermission.Should().BeFalse();
    }

    [Fact(DisplayName = "Should return false when tenant context is missing")]
    [Trait("Layer", "Application")]
    [Trait("Category", "Authorization")]
    public async Task Should_Return_False_When_TenantContext_Is_Missing()
    {
        // Arrange
        var service = CreateService(
            currentUserContext: new FakeCurrentUserContext
            {
                IsAuthenticated = true,
                UserId = Guid.NewGuid()
            },
            currentTenantContext: new FakeCurrentTenantContext
            {
                CurrentTenantId = null
            });

        // Act
        var hasPermission = await service.HasPermissionAsync(
            PermissionCode.Create("Identity.Users.Read"));

        // Assert
        hasPermission.Should().BeFalse();
    }

    [Fact(DisplayName = "Should return false when user has no roles")]
    [Trait("Layer", "Application")]
    [Trait("Category", "Authorization")]
    public async Task Should_Return_False_When_User_Has_No_Roles()
    {
        // Arrange
        var tenantId = TenantId.New();

        var service = CreateService(
            currentUserContext: new FakeCurrentUserContext
            {
                IsAuthenticated = true,
                UserId = Guid.NewGuid(),
                TenantId = tenantId.Value
            },
            currentTenantContext: new FakeCurrentTenantContext
            {
                CurrentTenantId = tenantId
            },
            roleRepository: new FakeRoleRepository
            {
                Roles = []
            });

        // Act
        var hasPermission = await service.HasPermissionAsync(
            PermissionCode.Create("Identity.Users.Read"));

        // Assert
        hasPermission.Should().BeFalse();
    }

    [Fact(DisplayName = "Should return false when user role does not have permission")]
    [Trait("Layer", "Application")]
    [Trait("Category", "Authorization")]
    public async Task Should_Return_False_When_User_Role_Does_Not_Have_Permission()
    {
        // Arrange
        var tenantId = TenantId.New();

        var role = Role.Create(
            tenantId,
            RoleName.Create("ReadOnly"),
            new DateTimeOffset(2026, 05, 07, 12, 0, 0, TimeSpan.Zero));

        var service = CreateService(
            currentUserContext: new FakeCurrentUserContext
            {
                IsAuthenticated = true,
                UserId = Guid.NewGuid(),
                TenantId = tenantId.Value
            },
            currentTenantContext: new FakeCurrentTenantContext
            {
                CurrentTenantId = tenantId
            },
            roleRepository: new FakeRoleRepository
            {
                Roles = [role]
            });

        // Act
        var hasPermission = await service.HasPermissionAsync(
            PermissionCode.Create("Identity.Users.Read"));

        // Assert
        hasPermission.Should().BeFalse();
    }

    [Fact(DisplayName = "Should return false when role is inactive")]
    [Trait("Layer", "Application")]
    [Trait("Category", "Authorization")]
    public async Task Should_Return_False_When_Role_Is_Inactive()
    {
        // Arrange
        var tenantId = TenantId.New();

        var permission = CreatePermission("Identity.Users.Read");

        var role = Role.Create(
            tenantId,
            RoleName.Create("Admin"),
            new DateTimeOffset(2026, 05, 07, 12, 0, 0, TimeSpan.Zero));

        role.GrantPermission(permission);
        role.Deactivate();

        var service = CreateService(
            currentUserContext: new FakeCurrentUserContext
            {
                IsAuthenticated = true,
                UserId = Guid.NewGuid(),
                TenantId = tenantId.Value
            },
            currentTenantContext: new FakeCurrentTenantContext
            {
                CurrentTenantId = tenantId
            },
            roleRepository: new FakeRoleRepository
            {
                Roles = [role]
            });

        // Act
        var hasPermission = await service.HasPermissionAsync(permission.Code);

        // Assert
        hasPermission.Should().BeFalse();
    }

    private static PermissionAuthorizationService CreateService(
        FakeCurrentUserContext? currentUserContext = null,
        FakeCurrentTenantContext? currentTenantContext = null,
        FakeRoleRepository? roleRepository = null)
    {
        var tenantId = TenantId.New();

        return new PermissionAuthorizationService(
            currentUserContext ?? new FakeCurrentUserContext
            {
                IsAuthenticated = true,
                UserId = Guid.NewGuid(),
                TenantId = tenantId.Value
            },
            currentTenantContext ?? new FakeCurrentTenantContext
            {
                CurrentTenantId = tenantId
            },
            roleRepository ?? new FakeRoleRepository());
    }

    private static Permission CreatePermission(string code)
    {
        return Permission.Create(
            PermissionCode.Create(code),
            $"Permission {code}.",
            new DateTimeOffset(2026, 05, 07, 12, 0, 0, TimeSpan.Zero));
    }

    private sealed class FakeCurrentUserContext : ICurrentUserContext
    {
        public bool IsAuthenticated { get; init; }

        public Guid? UserId { get; init; }

        public Guid? TenantId { get; init; }

        public string? Name { get; init; }

        public string? Email { get; init; }
    }

    private sealed class FakeCurrentTenantContext : ICurrentTenantContext
    {
        public bool HasTenant => CurrentTenantId is not null;

        public TenantId? CurrentTenantId { get; init; }
    }

    private sealed class FakeRoleRepository : IRoleRepository
    {
        public IReadOnlyCollection<Role> Roles { get; init; } = [];

        public TenantId? LastTenantId { get; private set; }

        public UserId? LastUserId { get; private set; }

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
            return Task.FromResult<Role?>(null);
        }

        public Task<IReadOnlyCollection<Role>> GetByUserAsync(
            TenantId tenantId,
            UserId userId,
            CancellationToken cancellationToken = default)
        {
            LastTenantId = tenantId;
            LastUserId = userId;

            return Task.FromResult(Roles);
        }

        public Task AddAsync(
            Role role,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task UpdateAsync(
            Role role,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task AssignToUserAsync(
            UserRole userRole,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
