using AwesomeAssertions;
using Gauss.Identity.Domain.Roles;
using Gauss.Identity.Domain.Roles.ValueObjects;
using Gauss.Identity.Infrastructure.Persistence;
using Gauss.Testing.Fixtures;

namespace Gauss.Identity.InfrastructureTests.Persistence;

public sealed class SqlPermissionRepositoryTests(
    SqlServerTestDatabaseFixture databaseFixture)
    : IClassFixture<SqlServerTestDatabaseFixture>
{
    [Fact(DisplayName = "Should add and get permission by code")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Persistence")]
    public async Task Should_Add_And_Get_Permission_By_Code()
    {
        // Arrange
        var repository = CreateRepository();

        var permission = CreatePermission("Identity.Users.Read");

        // Act
        await repository.AddAsync(permission);

        var persistedPermission = await repository.GetByCodeAsync(permission.Code);

        // Assert
        persistedPermission.Should().NotBeNull();
        persistedPermission!.Id.Should().Be(permission.Id);
        persistedPermission.Code.Should().Be(permission.Code);
        persistedPermission.Description.Should().Be(permission.Description);
        persistedPermission.IsEnabled.Should().BeTrue();
        persistedPermission.CreatedAtUtc.Should().Be(permission.CreatedAtUtc);
    }

    [Fact(DisplayName = "Should return true when permission code exists")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Persistence")]
    public async Task Should_Return_True_When_PermissionCode_Exists()
    {
        // Arrange
        var repository = CreateRepository();

        var permission = CreatePermission("Identity.Users.Manage");

        await repository.AddAsync(permission);

        // Act
        var exists = await repository.ExistsByCodeAsync(permission.Code);

        // Assert
        exists.Should().BeTrue();
    }

    [Fact(DisplayName = "Should return false when permission code does not exist")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Persistence")]
    public async Task Should_Return_False_When_PermissionCode_Does_Not_Exist()
    {
        // Arrange
        var repository = CreateRepository();

        var permissionCode = PermissionCode.Create("Identity.Users.Missing");

        // Act
        var exists = await repository.ExistsByCodeAsync(permissionCode);

        // Assert
        exists.Should().BeFalse();
    }

    [Fact(DisplayName = "Should return null when permission code does not exist")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Persistence")]
    public async Task Should_Return_Null_When_PermissionCode_Does_Not_Exist()
    {
        // Arrange
        var repository = CreateRepository();

        var permissionCode = PermissionCode.Create("Identity.Users.Missing");

        // Act
        var permission = await repository.GetByCodeAsync(permissionCode);

        // Assert
        permission.Should().BeNull();
    }

    [Fact(DisplayName = "Should return all enabled permissions")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Persistence")]
    public async Task Should_Return_All_Enabled_Permissions()
    {
        // Arrange
        var repository = CreateRepository();

        var enabledPermission = CreatePermission("Identity.Users.Read");
        var disabledPermission = CreatePermission("Identity.Users.Delete");

        disabledPermission.Disable();

        await repository.AddAsync(enabledPermission);
        await repository.AddAsync(disabledPermission);

        // Act
        var permissions = await repository.GetAllEnabledAsync();

        // Assert
        permissions.Should().Contain(permission =>
            permission.Code == enabledPermission.Code);

        permissions.Should().NotContain(permission =>
            permission.Code == disabledPermission.Code);
    }

    [Fact(DisplayName = "Should update permission")]
    [Trait("Layer", "Infrastructure")]
    [Trait("Category", "Persistence")]
    public async Task Should_Update_Permission()
    {
        // Arrange
        var repository = CreateRepository();

        var permission = CreatePermission("Identity.Users.Update");

        await repository.AddAsync(permission);

        permission.UpdateDescription("Updated permission description.");
        permission.Disable();

        // Act
        await repository.UpdateAsync(permission);

        var persistedPermission = await repository.GetByCodeAsync(permission.Code);

        // Assert
        persistedPermission.Should().NotBeNull();
        persistedPermission!.Description.Should().Be("Updated permission description.");
        persistedPermission.IsEnabled.Should().BeFalse();
    }

    private SqlPermissionRepository CreateRepository()
    {
        var options = Microsoft.Extensions.Options.Options.Create(
            new IdentityPersistenceOptions
            {
                ConnectionString = databaseFixture.ConnectionString
            });

        var connectionFactory = new IdentityDbConnectionFactory(options);

        return new SqlPermissionRepository(connectionFactory);
    }

    private static Permission CreatePermission(string code)
    {
        return Permission.Create(
            PermissionCode.Create($"{code}.{Guid.NewGuid():N}"),
            $"Permission {code}.",
            new DateTimeOffset(2026, 05, 07, 12, 0, 0, TimeSpan.Zero));
    }
}
