using Gauss.Identity.Domain.Users.Tenancy;

namespace Gauss.Identity.Application.Abstractions.Tenancy;

public interface ICurrentTenantContext
{
    bool HasTenant { get; }

    TenantId? CurrentTenantId { get; }
}
