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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Tests.Hangman.Support
{
    public static class TestServiceCollections
    {
        public static void ConfigureServices_Default(IServiceCollection services) => ConfigureServices(services);

        public static void ConfigureServices_MockJwtSvc(IServiceCollection services)
        {
            ConfigureServices(services);

            var mockJwtSvcManager = new Mock<IJwtSvc>();
            var mockJwtSvc = mockJwtSvcManager.Object;

            services.AddScoped(sp => mockJwtSvcManager);
            services.AddScoped(sp => mockJwtSvc);
        }

        public static void ConfigureServices_FakeAuthHandler(IServiceCollection services)
        {
            ConfigureServices(services);

            // Adds a fake authentication handler scheme
            services.AddAuthentication(FakeAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, FakeAuthHandler>(FakeAuthHandler.SchemeName, options => { });
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            var config = BuildAppConfiguration();

            // SqlContext and DbContextOptions must be removed as they were inject with diff options!
            // if not explicitly removed, services.AddDbContext will just add ANOTHER SqlContext that's unwanted
            services.RemoveAll<SqlContext>();
            services.RemoveAll<DbContextOptions<SqlContext>>();
            services.AddDbContext<SqlContext>(options =>
                options.UseNpgsql(config.GetConnectionString("SqlConnection")),
                    ServiceLifetime.Singleton, ServiceLifetime.Singleton);
        }


        public class FakeAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
        {
            public const string SchemeName = "TestingScheme";

            public FakeAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
                ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
                : base(options, logger, encoder, clock) { }

            protected override Task<AuthenticateResult> HandleAuthenticateAsync()
            {
                // dummy authentication handler for 'TestingScheme'
                var claims = new[] { new Claim(ClaimTypes.Name, "Test user") };
                var identity = new ClaimsIdentity(claims, SchemeName);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, SchemeName);

                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
        }
        public static IConfigurationRoot BuildAppConfiguration()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            return configuration;
        }
    }
}