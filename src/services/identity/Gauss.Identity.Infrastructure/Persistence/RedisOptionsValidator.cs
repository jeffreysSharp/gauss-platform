using Microsoft.Extensions.Options;

namespace Gauss.Identity.Infrastructure.Persistence;

public sealed class RedisOptionsValidator
    : IValidateOptions<RedisOptions>
{
    public ValidateOptionsResult Validate(
        string? name,
        RedisOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            return ValidateOptionsResult.Fail(
                "Redis connection string was not configured.");
        }

        return ValidateOptionsResult.Success;
    }
}
