using System.Linq;
using Hangman.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace Tests.Hangman.Support
{
    public static class TestInjections
    {
        public static IConfigurationRoot BuildAppConfiguration()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            return configuration;
        }

        public static void AddDefaultServices(IServiceCollection services)
        {
            var config = BuildAppConfiguration();

            // SqlContext and DbContextOptions must be removed as they were inject with diff options!
            // if not explicitly removed, services.AddDbContext will just add ANOTHER SqlContext that's unwanted
            services.RemoveAll<SqlContext>();
            services.RemoveAll<DbContextOptions<SqlContext>>();
            services.AddDbContext<SqlContext>(options =>
                options.UseNpgsql(config.GetConnectionString("SqlConnection")), ServiceLifetime.Singleton, ServiceLifetime.Singleton);

            var descriptors = services.Select(d => d.ServiceType == typeof(IHostedService));

            // Hangfire and other IHostedServices are removed
            //services.RemoveAll<IHostedService>();
        }

        public static void DefaultConfiguration(IServiceCollection services)
        {
            AddDefaultServices(services);
        }
    }
}