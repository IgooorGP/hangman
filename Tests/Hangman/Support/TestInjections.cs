using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Hangman.Core.Services;
using Hangman.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

        public static void DefaultConfigurationNoAuth(IServiceCollection services)
        {
            AddDefaultServices(services);

            // Adds a dummy testing authentication scheme
            services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
        }

        public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
        {
            public const string TestScheme = "Test";

            public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
                ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
                : base(options, logger, encoder, clock) { }

            protected override Task<AuthenticateResult> HandleAuthenticateAsync()
            {
                var claims = new[] { new Claim(ClaimTypes.Name, "Test user") };
                var identity = new ClaimsIdentity(claims, TestScheme);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, TestScheme);

                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
        }
    }
}