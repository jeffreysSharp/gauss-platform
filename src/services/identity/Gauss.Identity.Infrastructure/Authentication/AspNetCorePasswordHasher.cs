using Gauss.Identity.Application.Abstractions.Authentication;
using Gauss.Identity.Domain.Users.ValueObjects;
using Microsoft.AspNetCore.Identity;

namespace Gauss.Identity.Infrastructure.Authentication;

public sealed class AspNetCorePasswordHasher : IPasswordHasher
{
    private static readonly object HashingContext = new();

    private readonly PasswordHasher<object> _passwordHasher = new();

    public PasswordHash Hash(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var hashedPassword = _passwordHasher.HashPassword(
            HashingContext,
            password);

        return PasswordHash.Create(hashedPassword);
    }

    public PasswordVerificationStatus Verify(
        PasswordHash passwordHash,
        string providedPassword)
    {
        ArgumentNullException.ThrowIfNull(passwordHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(providedPassword);

        var verificationResult = _passwordHasher.VerifyHashedPassword(
            HashingContext,
            passwordHash.Value,
            providedPassword);

        return verificationResult switch
        {
            PasswordVerificationResult.Success =>
                PasswordVerificationStatus.Success,

            PasswordVerificationResult.SuccessRehashNeeded =>
                PasswordVerificationStatus.SuccessRehashNeeded,

            PasswordVerificationResult.Failed =>
                PasswordVerificationStatus.Failed,

            _ => PasswordVerificationStatus.Failed
        };
    }
}
