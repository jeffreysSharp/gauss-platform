using AwesomeAssertions;
using Dapper;
using Gauss.Identity.Domain.Roles;
using Gauss.Identity.Domain.Roles.ValueObjects;
using Gauss.Identity.Domain.Tenants;
using Gauss.Identity.Domain.Users;
using Gauss.Identity.Domain.Users.ValueObjects;
using Gauss.Identity.Infrastructure.Persistence;
using Gauss.Identity.InfrastructureTests.Fixtures;

namespace Gauss.Identity.InfrastructureTests.Persistence;

public sealed class SqlRoleRepositoryTests(
    SqlServerTestDatabaseFixture databaseFixture)
    : IClassFixture<SqlServerTestDatabaseFixture>
{
    [Fact(DisplayName = "Should add and get role by id")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Persistence")]
    public async Task Should_Add_And_Get_Role_By_Id()
    {
        // Arrange
        var roleRepository = CreateRoleRepository();
        var permissionRepository = CreatePermissionRepository();

        var tenantId = TenantId.New();

        await AddTenantAsync(tenantId);

        var permission = CreatePermission("Identity.Users.Read");

        await permissionRepository.AddAsync(permission);

        var role = CreateRole(
            tenantId,
            "Admin");

        role.GrantPermission(permission);

        // Act
        await roleRepository.AddAsync(role);

        var persistedRole = await roleRepository.GetByIdAsync(role.Id);

        // Assert
        persistedRole.Should().NotBeNull();

        persistedRole!.Id.Should().Be(role.Id);
        persistedRole.TenantId.Should().Be(role.TenantId);
        persistedRole.Name.Should().Be(role.Name);
        persistedRole.Status.Should().Be(role.Status);
        persistedRole.CreatedAtUtc.Should().Be(role.CreatedAtUtc);

        persistedRole.Permissions.Should().ContainSingle();

        var rolePermission = persistedRole.Permissions.Single();

        rolePermission.RoleId.Should().Be(role.Id);
        rolePermission.PermissionId.Should().Be(permission.Id);
        rolePermission.PermissionCode.Should().Be(permission.Code);
    }

    [Fact(DisplayName = "Should return true when role name exists in tenant")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Persistence")]
    public async Task Should_Return_True_When_RoleName_Exists_In_Tenant()
    {
        // Arrange
        var roleRepository = CreateRoleRepository();

        var tenantId = TenantId.New();

        await AddTenantAsync(tenantId);

        var role = CreateRole(
            tenantId,
            "Admin");

        await roleRepository.AddAsync(role);

        // Act
        var exists = await roleRepository.ExistsByNameAsync(
            tenantId,
            role.Name);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact(DisplayName = "Should return false when role name does not exist in tenant")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Persistence")]
    public async Task Should_Return_False_When_RoleName_Does_Not_Exist_In_Tenant()
    {
        // Arrange
        var roleRepository = CreateRoleRepository();

        var tenantId = TenantId.New();

        await AddTenantAsync(tenantId);

        // Act
        var exists = await roleRepository.ExistsByNameAsync(
            tenantId,
            RoleName.Create("MissingRole"));

        // Assert
        exists.Should().BeFalse();
    }

    [Fact(DisplayName = "Should isolate role name by tenant")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Persistence")]
    public async Task Should_Isolate_RoleName_By_Tenant()
    {
        // Arrange
        var roleRepository = CreateRoleRepository();

        var firstTenantId = TenantId.New();
        var secondTenantId = TenantId.New();

        await AddTenantAsync(firstTenantId);
        await AddTenantAsync(secondTenantId);

        var role = CreateRole(
            firstTenantId,
            "Admin");

        await roleRepository.AddAsync(role);

        // Act
        var existsInFirstTenant = await roleRepository.ExistsByNameAsync(
            firstTenantId,
            role.Name);

        var existsInSecondTenant = await roleRepository.ExistsByNameAsync(
            secondTenantId,
            role.Name);

        // Assert
        existsInFirstTenant.Should().BeTrue();
        existsInSecondTenant.Should().BeFalse();
    }

    [Fact(DisplayName = "Should update role")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Persistence")]
    public async Task Should_Update_Role()
    {
        // Arrange
        var roleRepository = CreateRoleRepository();

        var tenantId = TenantId.New();

        await AddTenantAsync(tenantId);

        var role = CreateRole(
            tenantId,
            "Admin");

        await roleRepository.AddAsync(role);

        role.Rename(RoleName.Create("Manager"));
        role.Deactivate();

        // Act
        await roleRepository.UpdateAsync(role);

        var persistedRole = await roleRepository.GetByIdAsync(role.Id);

        // Assert
        persistedRole.Should().NotBeNull();
        persistedRole!.Name.Should().Be(RoleName.Create("Manager"));
        persistedRole.Status.Should().Be(RoleStatus.Inactive);
        persistedRole.IsActive.Should().BeFalse();
    }

    [Fact(DisplayName = "Should persist role permissions on update")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Persistence")]
    public async Task Should_Persist_RolePermissions_On_Update()
    {
        // Arrange
        var roleRepository = CreateRoleRepository();
        var permissionRepository = CreatePermissionRepository();

        var tenantId = TenantId.New();

        await AddTenantAsync(tenantId);

        var readPermission = CreatePermission("Identity.Users.Read");
        var managePermission = CreatePermission("Identity.Users.Manage");

        await permissionRepository.AddAsync(readPermission);
        await permissionRepository.AddAsync(managePermission);

        var role = CreateRole(
            tenantId,
            "Admin");

        role.GrantPermission(readPermission);

        await roleRepository.AddAsync(role);

        role.GrantPermission(managePermission);

        // Act
        await roleRepository.UpdateAsync(role);

        var persistedRole = await roleRepository.GetByIdAsync(role.Id);

        // Assert
        persistedRole.Should().NotBeNull();
        persistedRole!.Permissions.Should().HaveCount(2);

        persistedRole.HasPermission(readPermission.Code).Should().BeTrue();
        persistedRole.HasPermission(managePermission.Code).Should().BeTrue();
    }

    [Fact(DisplayName = "Should assign role to user")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Persistence")]
    public async Task Should_Assign_Role_To_User()
    {
        // Arrange
        var userRepository = CreateUserRepository();
        var roleRepository = CreateRoleRepository();

        var user = CreateActiveUser();

        await userRepository.AddAsync(user);

        var role = CreateRole(
            user.TenantId,
            "Admin");

        await roleRepository.AddAsync(role);

        var userRole = UserRole.Assign(
            user.Id,
            user.TenantId,
            role.Id,
            new DateTimeOffset(2026, 05, 07, 12, 30, 0, TimeSpan.Zero));

        // Act
        await roleRepository.AssignToUserAsync(userRole);

        var roles = await roleRepository.GetByUserAsync(
            user.TenantId,
            user.Id);

        // Assert
        roles.Should().ContainSingle();

        var assignedRole = roles.Single();

        assignedRole.Id.Should().Be(role.Id);
        assignedRole.TenantId.Should().Be(user.TenantId);
        assignedRole.Name.Should().Be(role.Name);
    }

    [Fact(DisplayName = "Should return roles by user with permissions")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Persistence")]
    public async Task Should_Return_Roles_By_User_With_Permissions()
    {
        // Arrange
        var userRepository = CreateUserRepository();
        var roleRepository = CreateRoleRepository();
        var permissionRepository = CreatePermissionRepository();

        var user = CreateActiveUser();

        await userRepository.AddAsync(user);

        var permission = CreatePermission("Identity.Users.Read");

        await permissionRepository.AddAsync(permission);

        var role = CreateRole(
            user.TenantId,
            "Admin");

        role.GrantPermission(permission);

        await roleRepository.AddAsync(role);

        var userRole = UserRole.Assign(
            user.Id,
            user.TenantId,
            role.Id,
            new DateTimeOffset(2026, 05, 07, 12, 30, 0, TimeSpan.Zero));

        await roleRepository.AssignToUserAsync(userRole);

        // Act
        var roles = await roleRepository.GetByUserAsync(
            user.TenantId,
            user.Id);

        // Assert
        roles.Should().ContainSingle();

        var assignedRole = roles.Single();

        assignedRole.Id.Should().Be(role.Id);
        assignedRole.Permissions.Should().ContainSingle();
        assignedRole.HasPermission(permission.Code).Should().BeTrue();
    }

    private async Task AddTenantAsync(TenantId tenantId)
    {
        await using var connection = new Microsoft.Data.SqlClient.SqlConnection(
            databaseFixture.ConnectionString);

        const string sql = """
            INSERT INTO [platform].[Tenants]
            (
                [Id],
                [Name],
                [Slug],
                [Status],
                [CreatedAtUtc],
                [UpdatedAtUtc],
                [IsDeleted]
            )
            VALUES
            (
                @Id,
                @Name,
                @Slug,
                @Status,
                @CreatedAtUtc,
                NULL,
                0
            );
            """;

        await connection.ExecuteAsync(
            sql,
            new
            {
                Id = tenantId.Value,
                Name = $"Tenant {tenantId.Value:N}",
                Slug = tenantId.Value.ToString("N"),
                Status = 1,
                CreatedAtUtc = new DateTimeOffset(2026, 05, 07, 12, 0, 0, TimeSpan.Zero)
            });
    }

    private SqlRoleRepository CreateRoleRepository()
    {
        var options = Microsoft.Extensions.Options.Options.Create(
            new IdentityPersistenceOptions
            {
                ConnectionString = databaseFixture.ConnectionString
            });

        var connectionFactory = new IdentityDbConnectionFactory(options);

        return new SqlRoleRepository(connectionFactory);
    }

    private SqlPermissionRepository CreatePermissionRepository()
    {
        var options = Microsoft.Extensions.Options.Options.Create(
            new IdentityPersistenceOptions
            {
                ConnectionString = databaseFixture.ConnectionString
            });

        var connectionFactory = new IdentityDbConnectionFactory(options);

        return new SqlPermissionRepository(connectionFactory);
    }

    private SqlUserRepository CreateUserRepository()
    {
        var options = Microsoft.Extensions.Options.Options.Create(
            new IdentityPersistenceOptions
            {
                ConnectionString = databaseFixture.ConnectionString
            });

        var connectionFactory = new IdentityDbConnectionFactory(options);

        return new SqlUserRepository(connectionFactory);
    }

    private static Role CreateRole(
        TenantId tenantId,
        string name)
    {
        return Role.Create(
            tenantId,
            RoleName.Create($"{name}-{Guid.NewGuid():N}"),
            new DateTimeOffset(2026, 05, 07, 12, 0, 0, TimeSpan.Zero));
    }

    private static Permission CreatePermission(string code)
    {
        return Permission.Create(
            PermissionCode.Create($"{code}.{Guid.NewGuid():N}"),
            $"Permission {code}.",
            new DateTimeOffset(2026, 05, 07, 12, 0, 0, TimeSpan.Zero));
    }

    private static User CreateActiveUser()
    {
        var user = User.Register(
            TenantId.New(),
            "Jeferson Almeida",
            Email.Create($"jeferson-{Guid.NewGuid():N}@gauss.com"),
            PasswordHash.Create("hashed-password"),
            new DateTimeOffset(2026, 05, 07, 12, 0, 0, TimeSpan.Zero));

        user.ConfirmEmail(new DateTimeOffset(2026, 05, 07, 12, 5, 0, TimeSpan.Zero));

        return user;
    }
}
