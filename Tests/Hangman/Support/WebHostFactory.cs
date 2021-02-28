using System;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Tests.Hangman.Support
{
    /// <summary>
    /// Factory class for creating testing IWebHost servers for integration tests.
    /// </summary>
    /// <typeparam name="TStartup">Startup used to configure service collections.</typeparam>
    public class WebHostFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        private readonly Action<IServiceCollection> _configureTestServices;

        public WebHostFactory(Action<IServiceCollection> configureTestServices = null)
        {
            _configureTestServices = configureTestServices ?? (services => { });
        }

        protected override IHostBuilder CreateHostBuilder()
        {
            // Clears all providers in our IHost/IWebHosts to set reloadOnChange: false to avoid excess of 
            // file-os monitoring API usage on integration tests
            var builder = base.CreateHostBuilder()
                .ConfigureWebHost(builder =>
                {
                    // this builder is already configured to look for TStartup 
                    // (using UseStart() will invoke TStartup twice)
                    builder
                        .ConfigureAppConfiguration((hostingContext, configBuilder) =>
                        {
                            configBuilder.Sources.Clear();
                            configBuilder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
                        })
                        .ConfigureTestServices(_configureTestServices);  // overrides TStartup.ConfigureServices
                })
                .ConfigureAppConfiguration((hostingContext, configBuilder) =>
                {
                    configBuilder.Sources.Clear();
                    configBuilder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
                })
                .UseEnvironment("Testing")
                .UseSerilog();

            return builder;
        }
    }
}