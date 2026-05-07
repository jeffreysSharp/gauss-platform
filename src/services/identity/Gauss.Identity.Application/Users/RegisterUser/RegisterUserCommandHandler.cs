using Gauss.BuildingBlocks.Application.Abstractions.Messaging;
using Gauss.BuildingBlocks.Application.Abstractions.Results;
using Gauss.Identity.Application.Abstractions.Authentication;
using Gauss.Identity.Application.Abstractions.Persistence;
using Gauss.Identity.Application.Abstractions.Time;
using Gauss.Identity.Domain.Tenants;
using Gauss.Identity.Domain.Users;
using Gauss.Identity.Domain.Users.ValueObjects;

namespace Gauss.Identity.Application.Users.RegisterUser;

public sealed class RegisterUserCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<RegisterUserCommand, RegisterUserResponse>
{
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

        var tenantId = TenantId.New();
        var passwordHash = passwordHasher.Hash(command.Password);

        var user = User.Register(
            tenantId,
            command.Name,
            email,
            passwordHash,
            dateTimeProvider.UtcNow);

        await userRepository.AddAsync(user, cancellationToken);

        var response = new RegisterUserResponse(
            user.Id.Value,
            user.TenantId.Value,
            user.Name,
            user.Email.Value);

        return Result<RegisterUserResponse>.Success(response);
    }
}
