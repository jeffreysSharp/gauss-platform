using Gauss.Identity.Domain.Tenants;

namespace Gauss.Identity.Application.Abstractions.Tenancy;

public interface ITenantProvisioningService
{
    Task<TenantId> ProvisionAsync(
        string ownerName,
        DateTimeOffset createdAtUtc,
        CancellationToken cancellationToken = default);
}
