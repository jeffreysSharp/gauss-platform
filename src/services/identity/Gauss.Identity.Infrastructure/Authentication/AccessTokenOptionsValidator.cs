using Microsoft.Extensions.Options;

namespace Gauss.Identity.Infrastructure.Authentication;

public sealed class AccessTokenOptionsValidator
    : IValidateOptions<AccessTokenOptions>
{
    public ValidateOptionsResult Validate(
        string? name,
        AccessTokenOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.Issuer))
        {
            failures.Add("Access token issuer was not configured.");
        }

        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            failures.Add("Access token audience was not configured.");
        }

        if (string.IsNullOrWhiteSpace(options.SecretKey))
        {
            failures.Add("Access token secret key was not configured.");
        }
        else if (options.SecretKey.Length < AccessTokenOptions.MinimumSecretKeyLength)
        {
            failures.Add(
                $"Access token secret key must have at least {AccessTokenOptions.MinimumSecretKeyLength} characters.");
        }

        if (options.ExpirationMinutes <= 0)
        {
            failures.Add("Access token expiration must be greater than zero.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
