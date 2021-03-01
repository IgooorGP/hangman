using System.Linq;
using Hangman.Core.Services;
using Hangman.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Moq;

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
        }

        public static void DefaultConfiguration(IServiceCollection services)
        {
            AddDefaultServices(services);
        }

        public static void DefaultConfigurationWithMockJwtSvc(IServiceCollection services)
        {
            AddDefaultServices(services);

            var mockJwtSvcManager = new Mock<IJwtSvc>();
            var mockJwtSvc = mockJwtSvcManager.Object;

            services.AddScoped(sp => mockJwtSvcManager);
            services.AddScoped(sp => mockJwtSvc);
        }
    }
}