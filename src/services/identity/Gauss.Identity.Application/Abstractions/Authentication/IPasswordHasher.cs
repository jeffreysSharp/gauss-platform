using Gauss.Identity.Domain.Users.ValueObjects;

namespace Gauss.Identity.Application.Abstractions.Authentication;

public interface IPasswordHasher
{
    PasswordHash Hash(string password);

    PasswordVerificationStatus Verify(
        PasswordHash passwordHash,
        string providedPassword);
}
