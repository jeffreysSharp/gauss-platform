using Gauss.Identity.Domain.Roles;
using Gauss.Identity.Domain.Tenants;
using Gauss.Identity.Domain.Users;

namespace Gauss.Identity.Application.Abstractions.Provisioning;

public interface IRegistrationProvisioningService
{
    Task ProvisionAsync(
        TenantId tenantId,
        string tenantName,
        User user,
        Role adminRole,
        UserRole userRole,
        CancellationToken cancellationToken = default);
}
