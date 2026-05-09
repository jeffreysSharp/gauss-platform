using Gauss.BuildingBlocks.Domain.Tenants;
using Gauss.Identity.Application.Abstractions.Authentication;
using Gauss.Identity.Application.Abstractions.Tenancy;

namespace Gauss.Identity.Api.Authentication;

public sealed class HttpCurrentTenantContext(
    ICurrentUserContext currentUserContext)
    : ICurrentTenantContext
{
    public bool HasTenant => CurrentTenantId is not null;

    public TenantId? CurrentTenantId =>
        currentUserContext.TenantId.HasValue
            ? TenantId.From(currentUserContext.TenantId.Value)
            : null;
}
