using System;
using Hangman;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
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

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // builder is already configured to call TStartup automatically
            builder
                .ConfigureAppConfiguration((hostingContext, configBuilder) =>
                {
                    configBuilder.Sources.Clear();  // no json providers with reloadOnChange: true (no inotify API use)
                    configBuilder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
                })
                .UseSetting("Environment", "Testing")
                .ConfigureTestServices(_configureTestServices);
        }
    }
}