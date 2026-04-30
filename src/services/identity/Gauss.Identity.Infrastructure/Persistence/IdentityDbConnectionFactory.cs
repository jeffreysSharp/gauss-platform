using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Gauss.Identity.Infrastructure.Persistence;

public sealed class IdentityDbConnectionFactory(
    IOptions<IdentityPersistenceOptions> options)
{
    private readonly IdentityPersistenceOptions _options = options.Value;

    public SqlConnection CreateConnection()
    {
        if (string.IsNullOrWhiteSpace(_options.ConnectionString))
        {
            throw new InvalidOperationException(
                "Identity persistence connection string was not configured.");
        }

        return new SqlConnection(_options.ConnectionString);
    }
}
