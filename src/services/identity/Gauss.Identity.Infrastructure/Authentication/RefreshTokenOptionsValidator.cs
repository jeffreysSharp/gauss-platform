using Microsoft.Extensions.Options;

namespace Gauss.Identity.Infrastructure.Authentication;

public sealed class RefreshTokenOptionsValidator
    : IValidateOptions<RefreshTokenOptions>
{
    public ValidateOptionsResult Validate(
        string? name,
        RefreshTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.ExpirationMinutes < RefreshTokenOptions.MinimumExpirationMinutes)
        {
            return ValidateOptionsResult.Fail(
                $"Refresh token expiration must be greater than or equal to {RefreshTokenOptions.MinimumExpirationMinutes} minute.");
        }

        return ValidateOptionsResult.Success;
    }
}
