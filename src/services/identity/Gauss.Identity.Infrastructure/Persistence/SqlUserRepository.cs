using Dapper;
using Gauss.Identity.Application.Abstractions.Persistence;
using Gauss.Identity.Domain.Users;
using Gauss.Identity.Domain.Users.ValueObjects;

namespace Gauss.Identity.Infrastructure.Persistence;

public sealed class SqlUserRepository(
    IdentityDbConnectionFactory connectionFactory)
    : IUserRepository
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

    public async Task AddAsync(
        User user,
        CancellationToken cancellationToken = default)
    {
        const string insertTenantSql = """
            IF NOT EXISTS (
                SELECT 1
                FROM [platform].[Tenants]
                WHERE [Id] = @TenantId
            )
            BEGIN
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
                    @TenantId,
                    @TenantName,
                    @TenantSlug,
                    @TenantStatus,
                    @CreatedAtUtc,
                    NULL,
                    0
                );
            END
            """;

        const string insertUserSql = """
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
        await connection.OpenAsync(cancellationToken);

        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            var now = user.RegisteredAtUtc;

            var tenantSlug = user.TenantId.Value.ToString("N");

            var tenantParameters = new
            {
                TenantId = user.TenantId.Value,
                TenantName = $"Tenant {tenantSlug}",
                TenantSlug = tenantSlug,
                TenantStatus = 1,
                CreatedAtUtc = now
            };

            var userParameters = new
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
                CreatedAtUtc = now
            };

            await connection.ExecuteAsync(
                new CommandDefinition(
                    insertTenantSql,
                    tenantParameters,
                    transaction,
                    cancellationToken: cancellationToken));

            await connection.ExecuteAsync(
                new CommandDefinition(
                    insertUserSql,
                    userParameters,
                    transaction,
                    cancellationToken: cancellationToken));

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
