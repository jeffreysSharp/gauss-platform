using Dapper;
using Gauss.Identity.Application.Abstractions.Tenancy;
using Gauss.Identity.Domain.Tenants;
using Gauss.Identity.Infrastructure.Persistence;

namespace Gauss.Identity.Infrastructure.Tenancy;

public sealed class SqlTenantProvisioningService(
    IdentityDbConnectionFactory connectionFactory)
    : ITenantProvisioningService
{
    public async Task<TenantId> ProvisionAsync(
        string ownerName,
        DateTimeOffset createdAtUtc,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerName);

        var tenantId = TenantId.New();
        var tenantSlug = tenantId.Value.ToString("N");

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

        await using var connection = connectionFactory.CreateConnection();

        var command = new CommandDefinition(
            sql,
            new
            {
                Id = tenantId.Value,
                Name = ownerName,
                Slug = tenantSlug,
                Status = 1,
                CreatedAtUtc = createdAtUtc
            },
            cancellationToken: cancellationToken);

        await connection.ExecuteAsync(command);

        return tenantId;
    }
}
