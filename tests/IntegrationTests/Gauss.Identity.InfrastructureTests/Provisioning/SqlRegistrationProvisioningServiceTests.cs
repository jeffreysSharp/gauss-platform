using AwesomeAssertions;
using Dapper;
using Gauss.Identity.Domain.Roles;
using Gauss.Identity.Domain.Roles.ValueObjects;
using Gauss.Identity.Domain.Tenants;
using Gauss.Identity.Domain.Users;
using Gauss.Identity.Domain.Users.ValueObjects;
using Gauss.Identity.Infrastructure.Persistence;
using Gauss.Identity.Infrastructure.Provisioning;
using Gauss.Testing.Fixtures;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Gauss.Identity.InfrastructureTests.Provisioning;

public sealed class SqlRegistrationProvisioningServiceTests(
    SqlServerTestDatabaseFixture fixture)
    : IClassFixture<SqlServerTestDatabaseFixture>
{

    [Fact(DisplayName = "Should provision registration data atomically when data is valid")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Provisioning")]
    public async Task Should_Provision_Registration_Data_Atomically_When_Data_Is_Valid()
    {
        // Arrange
        var service = CreateService();

        var tenantId = TenantId.New();
        var tenantName = $"Tenant-{tenantId.Value:N}";

        var user = CreateUser(tenantId);
        var adminRole = CreateRole(tenantId);
        var userRole = UserRole.Assign(user.Id, tenantId, adminRole.Id, FixedUtcNow);

        await SeedPermissionsAsync();
        GrantBaselinePermissions(adminRole);

        // Act
        await service.ProvisionAsync(tenantId, tenantName, user, adminRole, userRole);

        // Assert
        await using var connection = new SqlConnection(fixture.ConnectionString);

        var tenantExists = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM [platform].[Tenants] WHERE [Id] = @Id",
            new { Id = tenantId.Value });

        var userExists = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM [identity].[Users] WHERE [Id] = @Id",
            new { Id = user.Id.Value });

        var roleExists = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM [identity].[Roles] WHERE [Id] = @Id",
            new { Id = adminRole.Id.Value });

        var userRoleExists = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM [identity].[UserRoles] WHERE [UserId] = @UserId AND [RoleId] = @RoleId",
            new { UserId = user.Id.Value, RoleId = adminRole.Id.Value });

        var rolePermissionCount = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM [identity].[RolePermissions] WHERE [RoleId] = @RoleId",
            new { RoleId = adminRole.Id.Value });

        var persistedTenantId = await connection.ExecuteScalarAsync<Guid>(
            "SELECT [TenantId] FROM [identity].[Users] WHERE [Id] = @Id",
            new { Id = user.Id.Value });

        tenantExists.Should().Be(1);
        userExists.Should().Be(1);
        roleExists.Should().Be(1);
        userRoleExists.Should().Be(1);
        rolePermissionCount.Should().BeGreaterThan(0);
        persistedTenantId.Should().Be(tenantId.Value);
    }

    [Fact(DisplayName = "Should rollback all registration data when provisioning fails")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Provisioning")]
    public async Task Should_Rollback_All_Registration_Data_When_Provisioning_Fails()
    {
        // Arrange
        var service = CreateService();

        var tenantId = TenantId.New();
        var tenantName = $"Tenant-{tenantId.Value:N}";

        var user = CreateUser(tenantId);
        var adminRole = CreateRole(tenantId);
        var userRole = UserRole.Assign(user.Id, tenantId, adminRole.Id, FixedUtcNow);

        await InsertTenantAsync(tenantId, tenantName);

        // Act
        var act = async () => await service.ProvisionAsync(
            tenantId, tenantName, user, adminRole, userRole);

        // Assert
        await act.Should().ThrowAsync<Exception>();

        await using var connection = new SqlConnection(fixture.ConnectionString);

        var tenantCount = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM [platform].[Tenants] WHERE [Id] = @Id",
            new { Id = tenantId.Value });

        var userExists = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM [identity].[Users] WHERE [Id] = @Id",
            new { Id = user.Id.Value });

        var roleExists = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM [identity].[Roles] WHERE [Id] = @Id",
            new { Id = adminRole.Id.Value });

        var userRoleExists = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM [identity].[UserRoles] WHERE [UserId] = @UserId AND [RoleId] = @RoleId",
            new { UserId = user.Id.Value, RoleId = adminRole.Id.Value });

        var rolePermissionCount = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM [identity].[RolePermissions] WHERE [RoleId] = @RoleId",
            new { RoleId = adminRole.Id.Value });

        tenantCount.Should().Be(1, "only the pre-inserted tenant row must exist; the transaction must not have added a second");
        userExists.Should().Be(0, "user must not be persisted after rollback");
        roleExists.Should().Be(0, "role must not be persisted after rollback");
        userRoleExists.Should().Be(0, "user-role assignment must not be persisted after rollback");
        rolePermissionCount.Should().Be(0, "role permissions must not be persisted after rollback");
    }

    [Fact(DisplayName = "Should reject provisioning when tenant ids do not match")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Provisioning")]
    public async Task Should_Reject_Provisioning_When_TenantIds_Do_Not_Match()
    {
        // Arrange
        var service = CreateService();

        var tenantId = TenantId.New();
        var differentTenantId = TenantId.New();

        var user = CreateUser(differentTenantId);
        var adminRole = CreateRole(tenantId);
        var userRole = UserRole.Assign(user.Id, tenantId, adminRole.Id, FixedUtcNow);

        // Act
        var act = async () => await service.ProvisionAsync(
            tenantId, $"Tenant-{tenantId.Value:N}", user, adminRole, userRole);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*User tenant does not match*");

        await using var connection = new SqlConnection(fixture.ConnectionString);

        var tenantExists = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM [platform].[Tenants] WHERE [Id] = @Id",
            new { Id = tenantId.Value });

        var userExists = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(1) FROM [identity].[Users] WHERE [Id] = @Id",
            new { Id = user.Id.Value });

        tenantExists.Should().Be(0, "no tenant must be created when tenant guard fails");
        userExists.Should().Be(0, "no user must be created when tenant guard fails");
    }

    private SqlRegistrationProvisioningService CreateService()
    {
        var options = Options.Create(new IdentityPersistenceOptions
        {
            ConnectionString = fixture.ConnectionString
        });

        return new SqlRegistrationProvisioningService(new IdentityDbConnectionFactory(options));
    }

    private static User CreateUser(TenantId tenantId)
    {
        return User.Register(
            tenantId,
            "Jeferson Almeida",
            Email.Create($"jeferson-{Guid.NewGuid():N}@gauss.com"),
            PasswordHash.Create("hashed-password-value"),
            FixedUtcNow);
    }

    private static Role CreateRole(TenantId tenantId)
    {
        return Role.Create(
            tenantId,
            RoleName.Create($"Tenant Administrator {Guid.NewGuid():N}"),
            FixedUtcNow);
    }

    private static readonly IReadOnlyList<(Guid Id, string Code, string Description)> BaselinePermissions =
    [
        (Guid.Parse("8d1b2f8a-14e3-4a2f-9a76-0a2f7f0b1c01"), "Identity.Users.Read",       "Allows reading identity users."),
        (Guid.Parse("6c0e8d61-0d84-4f7a-8c8d-95c0428e7e02"), "Identity.Users.Manage",     "Allows managing identity users."),
        (Guid.Parse("4e62d99b-6a9d-4f57-9d11-9d5b2b4e6a03"), "Identity.Roles.Read",       "Allows reading identity roles."),
        (Guid.Parse("9b7d9e5f-81de-4ef9-9a63-0c2a2e6f4d04"), "Identity.Roles.Manage",     "Allows managing identity roles."),
        (Guid.Parse("e6cbf8f0-1e77-4d62-a36e-75b29141f705"), "Identity.Permissions.Read", "Allows reading identity permissions."),
        (Guid.Parse("2b841c5e-63d4-4a96-8d93-4a0e20f27a06"), "Identity.Tenant.Read",      "Allows reading tenant information."),
        (Guid.Parse("7f0f64d4-55c1-4f5c-a89b-bc3f3cfe7107"), "Identity.Tenant.Manage",    "Allows managing tenant information."),
    ];

    private async Task SeedPermissionsAsync()
    {
        await using var connection = new SqlConnection(fixture.ConnectionString);

        const string sql = """
            IF NOT EXISTS (
                SELECT 1 FROM [identity].[Permissions] WHERE [Code] = @Code AND [IsDeleted] = 0
            )
            INSERT INTO [identity].[Permissions]
            ([Id], [Code], [Description], [IsEnabled], [CreatedAtUtc], [UpdatedAtUtc], [IsDeleted])
            VALUES (@Id, @Code, @Description, 1, @CreatedAtUtc, NULL, 0);
            """;

        foreach (var (id, code, description) in BaselinePermissions)
        {
            await connection.ExecuteAsync(sql, new
            {
                Id = id,
                Code = code,
                Description = description,
                CreatedAtUtc = FixedUtcNow
            });
        }
    }

    private static void GrantBaselinePermissions(Role role)
    {
        foreach (var (id, code, description) in BaselinePermissions)
        {
            var permission = Permission.Rehydrate(new PermissionSnapshot(
                PermissionId.From(id),
                PermissionCode.Create(code),
                description,
                IsEnabled: true,
                FixedUtcNow));

            role.GrantPermission(permission);
        }
    }

    private static readonly DateTimeOffset FixedUtcNow =
        new(2026, 05, 15, 12, 0, 0, TimeSpan.Zero);

    private async Task InsertTenantAsync(TenantId tenantId, string tenantName)
    {
        await using var connection = new SqlConnection(fixture.ConnectionString);

        const string sql = """
            INSERT INTO [platform].[Tenants]
            ([Id], [Name], [Slug], [Status], [CreatedAtUtc], [UpdatedAtUtc], [IsDeleted])
            VALUES (@Id, @Name, @Slug, @Status, @CreatedAtUtc, NULL, 0);
            """;

        await connection.ExecuteAsync(sql, new
        {
            Id = tenantId.Value,
            Name = tenantName,
            Slug = tenantId.Value.ToString("N"),
            Status = 1,
            CreatedAtUtc = FixedUtcNow
        });
    }
}
