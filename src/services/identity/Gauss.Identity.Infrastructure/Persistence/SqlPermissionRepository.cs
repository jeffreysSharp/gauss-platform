using Dapper;
using Gauss.Identity.Application.Abstractions.Persistence;
using Gauss.Identity.Domain.Roles;
using Gauss.Identity.Domain.Roles.ValueObjects;

namespace Gauss.Identity.Infrastructure.Persistence;

public sealed class SqlPermissionRepository(
    IdentityDbConnectionFactory connectionFactory)
    : IPermissionRepository
{
    public async Task<bool> ExistsByCodeAsync(
        PermissionCode code,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CAST(
                CASE
                    WHEN EXISTS (
                        SELECT 1
                        FROM [identity].[Permissions]
                        WHERE [Code] = @Code
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
                Code = code.Value
            },
            cancellationToken: cancellationToken);

        return await connection.ExecuteScalarAsync<bool>(command);
    }

    public async Task<Permission?> GetByCodeAsync(
        PermissionCode code,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                [Id],
                [Code],
                [Description],
                [IsEnabled],
                [CreatedAtUtc]
            FROM [identity].[Permissions]
            WHERE [Code] = @Code
              AND [IsDeleted] = 0;
            """;

        await using var connection = connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            sql,
            new
            {
                Code = code.Value
            },
            cancellationToken: cancellationToken);

        var record = await connection.QuerySingleOrDefaultAsync<PermissionPersistenceRecord>(command);

        return record is null
            ? null
            : MapToPermission(record);
    }

    public async Task<IReadOnlyCollection<Permission>> GetAllEnabledAsync(
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
                [Id],
                [Code],
                [Description],
                [IsEnabled],
                [CreatedAtUtc]
            FROM [identity].[Permissions]
            WHERE [IsEnabled] = 1
              AND [IsDeleted] = 0
            ORDER BY [Code];
            """;

        await using var connection = connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            sql,
            cancellationToken: cancellationToken);

        var records = await connection.QueryAsync<PermissionPersistenceRecord>(command);

        return records
            .Select(MapToPermission)
            .ToList();
    }

    public async Task AddAsync(
        Permission permission,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO [identity].[Permissions]
            (
                [Id],
                [Code],
                [Description],
                [IsEnabled],
                [CreatedAtUtc],
                [UpdatedAtUtc],
                [IsDeleted]
            )
            VALUES
            (
                @Id,
                @Code,
                @Description,
                @IsEnabled,
                @CreatedAtUtc,
                NULL,
                0
            );
            """;

        await using var connection = connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            sql,
            new
            {
                Id = permission.Id.Value,
                Code = permission.Code.Value,
                permission.Description,
                permission.IsEnabled,
                permission.CreatedAtUtc
            },
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }

    public async Task UpdateAsync(
        Permission permission,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE [identity].[Permissions]
            SET
                [Description] = @Description,
                [IsEnabled] = @IsEnabled,
                [UpdatedAtUtc] = @UpdatedAtUtc
            WHERE [Id] = @Id
              AND [IsDeleted] = 0;
            """;

        await using var connection = connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            sql,
            new
            {
                Id = permission.Id.Value,
                permission.Description,
                permission.IsEnabled,
                UpdatedAtUtc = DateTimeOffset.UtcNow
            },
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }

    private static Permission MapToPermission(
        PermissionPersistenceRecord record)
    {
        var snapshot = new PermissionSnapshot(
            PermissionId.From(record.Id),
            PermissionCode.Create(record.Code),
            record.Description,
            record.IsEnabled,
            record.CreatedAtUtc);

        return Permission.Rehydrate(snapshot);
    }

    private sealed record PermissionPersistenceRecord(
        Guid Id,
        string Code,
        string Description,
        bool IsEnabled,
        DateTimeOffset CreatedAtUtc);
}
