using Dapper;
using Gauss.BuildingBlocks.Domain.Tenants;
using Gauss.Identity.Application.Abstractions.Provisioning;
using Gauss.Identity.Domain.Roles;
using Gauss.Identity.Domain.Users;
using Gauss.Identity.Infrastructure.Persistence;

namespace Gauss.Identity.Infrastructure.Provisioning;

public sealed class SqlRegistrationProvisioningService(
    IdentityDbConnectionFactory connectionFactory)
    : IRegistrationProvisioningService
{
    public async Task ProvisionAsync(
        TenantId tenantId,
        string tenantName,
        User user,
        Role adminRole,
        UserRole userRole,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantName);
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(adminRole);
        ArgumentNullException.ThrowIfNull(userRole);

        EnsureSameTenant(
            tenantId,
            user,
            adminRole,
            userRole);

        await using var connection = connectionFactory.CreateConnection();

        await connection.OpenAsync(cancellationToken);

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await InsertTenantAsync(
                connection,
                transaction,
                tenantId,
                tenantName,
                user.RegisteredAtUtc,
                cancellationToken);

            await InsertUserAsync(
                connection,
                transaction,
                user,
                cancellationToken);

            await InsertRoleAsync(
                connection,
                transaction,
                adminRole,
                cancellationToken);

            await InsertRolePermissionsAsync(
                connection,
                transaction,
                adminRole,
                cancellationToken);

            await InsertUserRoleAsync(
                connection,
                transaction,
                userRole,
                cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);

            throw;
        }
    }

    private static void EnsureSameTenant(
        TenantId tenantId,
        User user,
        Role adminRole,
        UserRole userRole)
    {
        if (user.TenantId != tenantId)
        {
            throw new InvalidOperationException(
                "User tenant does not match the provisioned tenant.");
        }

        if (adminRole.TenantId != tenantId)
        {
            throw new InvalidOperationException(
                "Role tenant does not match the provisioned tenant.");
        }

        if (userRole.TenantId != tenantId)
        {
            throw new InvalidOperationException(
                "User role tenant does not match the provisioned tenant.");
        }

        if (userRole.UserId != user.Id)
        {
            throw new InvalidOperationException(
                "User role assignment does not match the registered user.");
        }

        if (userRole.RoleId != adminRole.Id)
        {
            throw new InvalidOperationException(
                "User role assignment does not match the administrator role.");
        }
    }

    private static async Task InsertTenantAsync(
        Microsoft.Data.SqlClient.SqlConnection connection,
        System.Data.Common.DbTransaction transaction,
        TenantId tenantId,
        string tenantName,
        DateTimeOffset createdAtUtc,
        CancellationToken cancellationToken)
    {
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
            new CommandDefinition(
                sql,
                new
                {
                    Id = tenantId.Value,
                    Name = tenantName,
                    Slug = tenantId.Value.ToString("N"),
                    Status = 1,
                    CreatedAtUtc = createdAtUtc
                },
                transaction,
                cancellationToken: cancellationToken));
    }

    private static async Task InsertUserAsync(
        Microsoft.Data.SqlClient.SqlConnection connection,
        System.Data.Common.DbTransaction transaction,
        User user,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO [identity].[Users]
            (
                [Id],
                [TenantId],
                [Name],
                [Email],
                [NormalizedEmail],
                [PasswordHash],
                [Status],
                [RegisteredAtUtc],
                [EmailConfirmedAtUtc],
                [LastLoginAtUtc],
                [LockedUntilUtc],
                [CreatedAtUtc],
                [UpdatedAtUtc],
                [IsDeleted]
            )
            VALUES
            (
                @Id,
                @TenantId,
                @Name,
                @Email,
                @NormalizedEmail,
                @PasswordHash,
                @Status,
                @RegisteredAtUtc,
                @EmailConfirmedAtUtc,
                @LastLoginAtUtc,
                @LockedUntilUtc,
                @CreatedAtUtc,
                NULL,
                0
            );
            """;

        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new
                {
                    Id = user.Id.Value,
                    TenantId = user.TenantId.Value,
                    user.Name,
                    Email = user.Email.Value,
                    NormalizedEmail = user.Email.Value,
                    PasswordHash = user.PasswordHash.Value,
                    Status = (int)user.Status,
                    user.RegisteredAtUtc,
                    user.EmailConfirmedAtUtc,
                    user.LastLoginAtUtc,
                    user.LockedUntilUtc,
                    CreatedAtUtc = user.RegisteredAtUtc
                },
                transaction,
                cancellationToken: cancellationToken));
    }

    private static async Task InsertRoleAsync(
        Microsoft.Data.SqlClient.SqlConnection connection,
        System.Data.Common.DbTransaction transaction,
        Role role,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO [identity].[Roles]
            (
                [Id],
                [TenantId],
                [Name],
                [Status],
                [CreatedAtUtc],
                [UpdatedAtUtc],
                [IsDeleted]
            )
            VALUES
            (
                @Id,
                @TenantId,
                @Name,
                @Status,
                @CreatedAtUtc,
                NULL,
                0
            );
            """;

        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new
                {
                    Id = role.Id.Value,
                    TenantId = role.TenantId.Value,
                    Name = role.Name.Value,
                    Status = (int)role.Status,
                    role.CreatedAtUtc
                },
                transaction,
                cancellationToken: cancellationToken));
    }

    private static async Task InsertRolePermissionsAsync(
        Microsoft.Data.SqlClient.SqlConnection connection,
        System.Data.Common.DbTransaction transaction,
        Role role,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO [identity].[RolePermissions]
            (
                [RoleId],
                [PermissionId],
                [PermissionCode],
                [CreatedAtUtc],
                [IsDeleted]
            )
            VALUES
            (
                @RoleId,
                @PermissionId,
                @PermissionCode,
                @CreatedAtUtc,
                0
            );
            """;

        foreach (var permission in role.Permissions)
        {
            await connection.ExecuteAsync(
                new CommandDefinition(
                    sql,
                    new
                    {
                        RoleId = permission.RoleId.Value,
                        PermissionId = permission.PermissionId.Value,
                        PermissionCode = permission.PermissionCode.Value,
                        CreatedAtUtc = role.CreatedAtUtc
                    },
                    transaction,
                    cancellationToken: cancellationToken));
        }
    }

    private static async Task InsertUserRoleAsync(
        Microsoft.Data.SqlClient.SqlConnection connection,
        System.Data.Common.DbTransaction transaction,
        UserRole userRole,
        CancellationToken cancellationToken)
    {
        const string sql = """
            INSERT INTO [identity].[UserRoles]
            (
                [UserId],
                [TenantId],
                [RoleId],
                [AssignedAtUtc],
                [IsDeleted]
            )
            VALUES
            (
                @UserId,
                @TenantId,
                @RoleId,
                @AssignedAtUtc,
                0
            );
            """;

        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new
                {
                    UserId = userRole.UserId.Value,
                    TenantId = userRole.TenantId.Value,
                    RoleId = userRole.RoleId.Value,
                    userRole.AssignedAtUtc
                },
                transaction,
                cancellationToken: cancellationToken));
    }
}
