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
                ["Identity:Persistence:ConnectionString"] = databaseFixture.ConnectionString
            };

            configurationBuilder.AddInMemoryCollection(configuration);
        });
    }
}
