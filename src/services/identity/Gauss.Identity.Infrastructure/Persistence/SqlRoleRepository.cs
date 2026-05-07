using Dapper;
using Gauss.Identity.Application.Abstractions.Persistence;
using Gauss.Identity.Domain.Roles;
using Gauss.Identity.Domain.Roles.ValueObjects;
using Gauss.Identity.Domain.Tenants;
using Gauss.Identity.Domain.Users;

namespace Gauss.Identity.Infrastructure.Persistence;

public sealed class SqlRoleRepository(
    IdentityDbConnectionFactory connectionFactory)
    : IRoleRepository
{
    public async Task<bool> ExistsByNameAsync(
        TenantId tenantId,
        RoleName name,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CAST(
                CASE
                    WHEN EXISTS (
                        SELECT 1
                        FROM [identity].[Roles]
                        WHERE [TenantId] = @TenantId
                          AND [Name] = @Name
                          AND [IsDeleted] = 0
                    )
                    THEN 1
                    ELSE 0
                END AS bit);
            """;

        await using var connection = connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            sql,
            new
            {
                TenantId = tenantId.Value,
                Name = name.Value
            },
            cancellationToken: cancellationToken);

        return await connection.ExecuteScalarAsync<bool>(command);
    }

    public async Task<Role?> GetByIdAsync(
        RoleId roleId,
        CancellationToken cancellationToken = default)
    {
        const string roleSql = """
            SELECT
                [Id],
                [TenantId],
                [Name],
                [Status],
                [CreatedAtUtc]
            FROM [identity].[Roles]
            WHERE [Id] = @Id
              AND [IsDeleted] = 0;
            """;

        const string permissionsSql = """
            SELECT
                [RoleId],
                [PermissionId],
                [PermissionCode]
            FROM [identity].[RolePermissions]
            WHERE [RoleId] = @RoleId
              AND [IsDeleted] = 0;
            """;

        await using var connection = connectionFactory.CreateConnection();

        var roleCommand = new CommandDefinition(
            roleSql,
            new
            {
                Id = roleId.Value
            },
            cancellationToken: cancellationToken);

        var roleRecord = await connection.QuerySingleOrDefaultAsync<RolePersistenceRecord>(
            roleCommand);

        if (roleRecord is null)
        {
            return null;
        }

        var permissionsCommand = new CommandDefinition(
            permissionsSql,
            new
            {
                RoleId = roleId.Value
            },
            cancellationToken: cancellationToken);

        var permissionRecords = await connection.QueryAsync<RolePermissionPersistenceRecord>(
            permissionsCommand);

        return MapToRole(
            roleRecord,
            permissionRecords);
    }

    public async Task<IReadOnlyCollection<Role>> GetByUserAsync(
        TenantId tenantId,
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        const string rolesSql = """
            SELECT
                r.[Id],
                r.[TenantId],
                r.[Name],
                r.[Status],
                r.[CreatedAtUtc]
            FROM [identity].[Roles] r
            INNER JOIN [identity].[UserRoles] ur
                ON ur.[RoleId] = r.[Id]
            WHERE ur.[TenantId] = @TenantId
              AND ur.[UserId] = @UserId
              AND ur.[IsDeleted] = 0
              AND r.[IsDeleted] = 0;
            """;

        const string permissionsSql = """
            SELECT
                rp.[RoleId],
                rp.[PermissionId],
                rp.[PermissionCode]
            FROM [identity].[RolePermissions] rp
            INNER JOIN [identity].[UserRoles] ur
                ON ur.[RoleId] = rp.[RoleId]
            WHERE ur.[TenantId] = @TenantId
              AND ur.[UserId] = @UserId
              AND ur.[IsDeleted] = 0
              AND rp.[IsDeleted] = 0;
            """;

        await using var connection = connectionFactory.CreateConnection();

        var parameters = new
        {
            TenantId = tenantId.Value,
            UserId = userId.Value
        };

        var rolesCommand = new CommandDefinition(
            rolesSql,
            parameters,
            cancellationToken: cancellationToken);

        var roleRecords = (await connection.QueryAsync<RolePersistenceRecord>(
                rolesCommand))
            .ToList();

        if (roleRecords.Count == 0)
        {
            return [];
        }

        var permissionsCommand = new CommandDefinition(
            permissionsSql,
            parameters,
            cancellationToken: cancellationToken);

        var permissionRecords = (await connection.QueryAsync<RolePermissionPersistenceRecord>(
                permissionsCommand))
            .ToList();

        var permissionsByRoleId = permissionRecords
            .GroupBy(permission => permission.RoleId)
            .ToDictionary(
                group => group.Key,
                group => group.AsEnumerable());

        return roleRecords
            .Select(roleRecord =>
            {
                permissionsByRoleId.TryGetValue(
                    roleRecord.Id,
                    out var rolePermissions);

                return MapToRole(
                    roleRecord,
                    rolePermissions ?? []);
            })
            .ToList();
    }

    public async Task AddAsync(
        Role role,
        CancellationToken cancellationToken = default)
    {
        const string insertRoleSql = """
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

        const string insertRolePermissionSql = """
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

        await using var connection = connectionFactory.CreateConnection();

        await connection.OpenAsync(cancellationToken);

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            var roleParameters = new
            {
                Id = role.Id.Value,
                TenantId = role.TenantId.Value,
                Name = role.Name.Value,
                Status = (int)role.Status,
                role.CreatedAtUtc
            };

            await connection.ExecuteAsync(
                new CommandDefinition(
                    insertRoleSql,
                    roleParameters,
                    transaction,
                    cancellationToken: cancellationToken));

            foreach (var permission in role.Permissions)
            {
                await connection.ExecuteAsync(
                    new CommandDefinition(
                        insertRolePermissionSql,
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

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);

            throw;
        }
    }

    public async Task UpdateAsync(
        Role role,
        CancellationToken cancellationToken = default)
    {
        const string updateRoleSql = """
            UPDATE [identity].[Roles]
            SET
                [Name] = @Name,
                [Status] = @Status,
                [UpdatedAtUtc] = @UpdatedAtUtc
            WHERE [Id] = @Id
              AND [IsDeleted] = 0;
            """;

        const string deleteRolePermissionsSql = """
             DELETE FROM [identity].[RolePermissions]
                WHERE [RoleId] = @RoleId;
           """;

        const string insertRolePermissionSql = """
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

        await using var connection = connectionFactory.CreateConnection();

        await connection.OpenAsync(cancellationToken);

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            await connection.ExecuteAsync(
                new CommandDefinition(
                    updateRoleSql,
                    new
                    {
                        Id = role.Id.Value,
                        Name = role.Name.Value,
                        Status = (int)role.Status,
                        UpdatedAtUtc = DateTimeOffset.UtcNow
                    },
                    transaction,
                    cancellationToken: cancellationToken));

            await connection.ExecuteAsync(
                new CommandDefinition(
                    deleteRolePermissionsSql,
                    new
                    {
                        RoleId = role.Id.Value
                    },
                    transaction,
                    cancellationToken: cancellationToken));

            foreach (var permission in role.Permissions)
            {
                await connection.ExecuteAsync(
                    new CommandDefinition(
                        insertRolePermissionSql,
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

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);

            throw;
        }
    }

    public async Task AssignToUserAsync(
        UserRole userRole,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            IF NOT EXISTS (
                SELECT 1
                FROM [identity].[UserRoles]
                WHERE [UserId] = @UserId
                  AND [TenantId] = @TenantId
                  AND [RoleId] = @RoleId
                  AND [IsDeleted] = 0
            )
            BEGIN
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
            END
            """;

        await using var connection = connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            sql,
            new
            {
                UserId = userRole.UserId.Value,
                TenantId = userRole.TenantId.Value,
                RoleId = userRole.RoleId.Value,
                userRole.AssignedAtUtc
            },
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }

    private static Role MapToRole(
        RolePersistenceRecord roleRecord,
        IEnumerable<RolePermissionPersistenceRecord> permissionRecords)
    {
        var snapshot = new RoleSnapshot(
            RoleId.From(roleRecord.Id),
            TenantId.From(roleRecord.TenantId),
            RoleName.Create(roleRecord.Name),
            (RoleStatus)roleRecord.Status,
            roleRecord.CreatedAtUtc);

        var permissions = permissionRecords.Select(permissionRecord =>
            new RolePermission(
                RoleId.From(permissionRecord.RoleId),
                PermissionId.From(permissionRecord.PermissionId),
                PermissionCode.Create(permissionRecord.PermissionCode)));

        return Role.Rehydrate(
            snapshot,
            permissions);
    }

    private sealed record RolePersistenceRecord(
        Guid Id,
        Guid TenantId,
        string Name,
        int Status,
        DateTimeOffset CreatedAtUtc);

    private sealed record RolePermissionPersistenceRecord(
        Guid RoleId,
        Guid PermissionId,
        string PermissionCode);
}
