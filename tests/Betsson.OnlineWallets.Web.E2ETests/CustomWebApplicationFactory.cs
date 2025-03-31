using Betsson.OnlineWallets.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Betsson.OnlineWallets.Web.E2ETests;

public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup>
    where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var logger = TestLoggingConfiguration.CreateLogger<CustomWebApplicationFactory<TStartup>>();

            logger.LogDebug($"{nameof(CustomWebApplicationFactory<TStartup>)}: " +
                $"Checking {nameof(OnlineWalletContext)} registrations...");

            var contextDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<OnlineWalletContext>));

            if (contextDescriptor != null)
            {
                logger.LogDebug($"Removing {nameof(DbContextOptions<OnlineWalletContext>)}: {contextDescriptor}");
                services.Remove(contextDescriptor);

                var optionsDescriptor = services
                    .Where(d =>
                        d.ServiceType == typeof(DbContextOptions) ||
                        (d.ServiceType.IsGenericType &&
                        d.ServiceType.GetGenericTypeDefinition() == typeof(DbContextOptions<>)))
                    .ToArray();

                foreach (var descriptor in optionsDescriptor)
                {
                    logger.LogDebug($"Removing: {descriptor}");
                    services.Remove(descriptor);
                }

                var optionsConfigDescriptor = services
                    .Where(d =>
                        d.ServiceType.IsGenericType &&
                        d.ServiceType.GetGenericTypeDefinition() == typeof(IDbContextOptionsConfiguration<>))
                    .ToArray();

                foreach (var descriptor in optionsConfigDescriptor)
                {
                    logger.LogDebug($"Removing: {descriptor}");
                    services.Remove(descriptor);
                }
            }

            logger.LogDebug($"Adding PostgreSQL {nameof(OnlineWalletContext)} registration.");
            services.AddDbContext<OnlineWalletContext>(options =>
            {
                var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

                options.UseNpgsql(connectionString);
            });
        });
    }
}
