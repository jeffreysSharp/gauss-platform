using Gauss.BuildingBlocks.Application.Abstractions.Messaging;
using Gauss.BuildingBlocks.Application.Abstractions.Results;
using Gauss.Identity.Application.Abstractions.Authentication;
using Gauss.Identity.Application.Abstractions.Persistence;
using Gauss.Identity.Application.Abstractions.Provisioning;
using Gauss.Identity.Application.Abstractions.Time;
using Gauss.Identity.Application.Authorization;
using Gauss.Identity.Domain.Roles;
using Gauss.Identity.Domain.Roles.ValueObjects;
using Gauss.Identity.Domain.Tenants;
using Gauss.Identity.Domain.Users;
using Gauss.Identity.Domain.Users.ValueObjects;

namespace Gauss.Identity.Application.Users.RegisterUser;

public sealed class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IPermissionRepository permissionRepository,
    IRegistrationProvisioningService registrationProvisioningService,
    IPasswordHasher passwordHasher,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<RegisterUserCommand, RegisterUserResponse>
{
    private const string InitialAdminRoleName = "Tenant Administrator";

    public async Task<Result<RegisterUserResponse>> HandleAsync(
        RegisterUserCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Email email;

        try
        {
            email = Email.Create(command.Email);
        }
        catch (ArgumentException)
        {
            return Result<RegisterUserResponse>.Failure(RegisterUserErrors.InvalidEmail);
        }

        var emailAlreadyExists = await userRepository.ExistsByEmailAsync(
            email,
            cancellationToken);

        if (emailAlreadyExists)
        {
            return Result<RegisterUserResponse>.Failure(RegisterUserErrors.EmailAlreadyExists);
        }

        var utcNow = dateTimeProvider.UtcNow;
        var tenantId = TenantId.New();

        var passwordHash = passwordHasher.Hash(command.Password);

        var user = User.Register(
            tenantId,
            command.Name,
            email,
            passwordHash,
            utcNow);

        var adminRole = Role.Create(
            tenantId,
            RoleName.Create(InitialAdminRoleName),
            utcNow);

        await GrantBaselinePermissionsAsync(
            adminRole,
            cancellationToken);

        var userRole = UserRole.Assign(
            user.Id,
            tenantId,
            adminRole.Id,
            utcNow);

        await registrationProvisioningService.ProvisionAsync(
            tenantId,
            command.Name,
            user,
            adminRole,
            userRole,
            cancellationToken);

        var response = new RegisterUserResponse(
            user.Id.Value,
            user.TenantId.Value,
            user.Name,
            user.Email.Value);

        return Result<RegisterUserResponse>.Success(response);
    }

    private async Task GrantBaselinePermissionsAsync(
        Role role,
        CancellationToken cancellationToken)
    {
        foreach (var permissionCode in GetBaselinePermissionCodes())
        {
            var permission = await permissionRepository.GetByCodeAsync(
                PermissionCode.Create(permissionCode),
                cancellationToken);

            if (permission is not null)
            {
                role.GrantPermission(permission);
            }
        }
    }

    private static IReadOnlyCollection<string> GetBaselinePermissionCodes()
    {
        return
        [
            IdentityPermissions.UsersRead,
            IdentityPermissions.UsersManage,
            IdentityPermissions.RolesRead,
            IdentityPermissions.RolesManage,
            IdentityPermissions.PermissionsRead,
            IdentityPermissions.TenantRead,
            IdentityPermissions.TenantManage
        ];
    }
}
