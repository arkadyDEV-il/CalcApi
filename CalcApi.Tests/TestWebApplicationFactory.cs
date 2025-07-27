using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace CalcApi.Tests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Ensure Development environment so dev-only routes (like /auth/token) are available
        builder.UseEnvironment(Environments.Development);

        // Override configuration for tests: inject a known, safe dev key & JWT settings
        builder.ConfigureAppConfiguration((ctx, cfg) =>
        {
            var overrides = new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "TEST_KEY_32_CHARS_MIN_TEST_KEY_32_CHARS_MIN",
                ["Jwt:Issuer"] = "CalcApi",
                ["Jwt:Audience"] = "CalcApiClients",
                ["Jwt:ExpiryMinutes"] = "60"
            };

            cfg.AddInMemoryCollection(overrides);
        });
    }
}
