using Gauss.BuildingBlocks.Domain.Tenants;

namespace Gauss.Identity.Application.Abstractions.Tenancy;

public interface ICurrentTenantContext
{
    bool HasTenant { get; }

    TenantId? CurrentTenantId { get; }
}
