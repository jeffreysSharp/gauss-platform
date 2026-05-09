using Dapper;
using Gauss.BuildingBlocks.Domain.Tenants;
using Gauss.Identity.Application.Abstractions.Persistence;
using Gauss.Identity.Domain.Users;
using Gauss.Identity.Domain.Users.ValueObjects;

namespace Gauss.Identity.Infrastructure.Persistence;

public sealed class SqlUserRepository(
    IdentityDbConnectionFactory connectionFactory) : IUserRepository
{
    public async Task<bool> ExistsByEmailAsync(
        Email email,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CAST(
                CASE
                    WHEN EXISTS (
                        SELECT 1
                        FROM [identity].[Users]
                        WHERE [NormalizedEmail] = @NormalizedEmail
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
                NormalizedEmail = email.Value
            },
            cancellationToken: cancellationToken);

        return await connection.ExecuteScalarAsync<bool>(command);
    }

    public async Task<User?> GetByEmailAsync(
        Email email,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
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
                [LockedUntilUtc]
            FROM [identity].[Users]
            WHERE [NormalizedEmail] = @NormalizedEmail
              AND [IsDeleted] = 0;
            """;

        await using var connection = connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            sql,
            new
            {
                NormalizedEmail = email.Value
            },
            cancellationToken: cancellationToken);

        var record = await connection.QuerySingleOrDefaultAsync<UserPersistenceRecord>(command);

        return record is null
            ? null
            : MapToUser(record);
    }

    public async Task<User?> GetByIdAsync(
        UserId userId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT
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
                [LockedUntilUtc]
            FROM [identity].[Users]
            WHERE [Id] = @Id
              AND [IsDeleted] = 0;
            """;

        await using var connection = connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            sql,
            new
            {
                Id = userId.Value
            },
            cancellationToken: cancellationToken);

        var record = await connection.QuerySingleOrDefaultAsync<UserPersistenceRecord>(command);

        return record is null
            ? null
            : MapToUser(record);
    }

    public async Task AddAsync(
        User user,
        CancellationToken cancellationToken = default)
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

        await using var connection = connectionFactory.CreateConnection();

        var command = new CommandDefinition(
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
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }

    public async Task RecordLoginAsync(
        UserId userId,
        DateTimeOffset loggedInAtUtc,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE [identity].[Users]
            SET
                [LastLoginAtUtc] = @LoggedInAtUtc,
                [UpdatedAtUtc] = @LoggedInAtUtc
            WHERE [Id] = @UserId
              AND [IsDeleted] = 0;
            """;

        await using var connection = connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            sql,
            new
            {
                UserId = userId.Value,
                LoggedInAtUtc = loggedInAtUtc
            },
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }

    public async Task UpdatePasswordHashAsync(
        UserId userId,
        PasswordHash passwordHash,
        DateTimeOffset updatedAtUtc,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE [identity].[Users]
            SET
                [PasswordHash] = @PasswordHash,
                [UpdatedAtUtc] = @UpdatedAtUtc
            WHERE [Id] = @UserId
              AND [IsDeleted] = 0;
            """;

        await using var connection = connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            sql,
            new
            {
                UserId = userId.Value,
                PasswordHash = passwordHash.Value,
                UpdatedAtUtc = updatedAtUtc
            },
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);
    }

    private static User MapToUser(
        UserPersistenceRecord record)
    {
        var snapshot = new UserSnapshot(
            UserId.From(record.Id),
            TenantId.From(record.TenantId),
            record.Name,
            Email.Create(record.Email),
            PasswordHash.Create(record.PasswordHash),
            (UserStatus)record.Status,
            record.RegisteredAtUtc,
            record.EmailConfirmedAtUtc,
            record.LastLoginAtUtc,
            record.LockedUntilUtc);

        return User.Rehydrate(snapshot);
    }

    private sealed record UserPersistenceRecord(
        Guid Id,
        Guid TenantId,
        string Name,
        string Email,
        string NormalizedEmail,
        string PasswordHash,
        int Status,
        DateTimeOffset RegisteredAtUtc,
        DateTimeOffset? EmailConfirmedAtUtc,
        DateTimeOffset? LastLoginAtUtc,
        DateTimeOffset? LockedUntilUtc);
}
