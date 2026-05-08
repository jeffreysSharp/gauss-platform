using Gauss.Testing.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Gauss.Identity.ApiTests.Fixtures;

public sealed class IdentityApiFactory(
    SqlServerTestDatabaseFixture databaseFixture)
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            var configuration = new Dictionary<string, string?>
            {
                ["Identity:Persistence:ConnectionString"] = databaseFixture.ConnectionString,

                ["Identity:AccessToken:Issuer"] = "GAUSS.Identity",
                ["Identity:AccessToken:Audience"] = "GAUSS.Platform",
                ["Identity:AccessToken:SecretKey"] = "test-only-secret-key-with-at-least-32-characters",
                ["Identity:AccessToken:ExpirationMinutes"] = "15",

                ["Identity:RefreshToken:ExpirationMinutes"] = "10080",
                ["Identity:Redis:ConnectionString"] =
                    Environment.GetEnvironmentVariable("GAUSS_TEST_REDIS_CONNECTION_STRING")
                    ?? "localhost:6379,abortConnect=false"
            };

            configurationBuilder.AddInMemoryCollection(configuration);
        });
    }
}
