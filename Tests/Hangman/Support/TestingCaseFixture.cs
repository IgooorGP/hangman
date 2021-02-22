using System;
using System.Net.Http;
using Hangman.Infrastructure;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Tests.Hangman.Support
{
    /**
     * Testing Case class which opens a DB transaction and rolls it back on the end of every
     * test in order have a clean database for every executed test.
     */
    public class TestingCaseFixture<TStartup> : IDisposable where TStartup : class
    {
        // private testing properties
        private readonly IDbContextTransaction _transaction;

        // properties used by testing classes
        protected readonly HttpClient Client;
        protected SqlContext DbContext { get; }

        protected TestingCaseFixture()
        {
            var builder = WebHost.CreateDefaultBuilder()
                .UseStartup<TStartup>()
                .UseSerilog();

            // constructs the testing server with the WebHostBuilder configuration
            // Startup class configures injected mocked services, and middleware (ConfigureServices, etc.)  
            var server = new TestServer(builder);
            var services = server.Host.Services;

            // resolve a DbContext instance from the container and begin a transaction on the context.
            Client = server.CreateClient();
            DbContext = services.GetRequiredService<SqlContext>();
            _transaction = DbContext.Database.BeginTransaction();
        }

        public void Dispose()
        {
            if (_transaction == null) return;

            _transaction.Rollback();
            _transaction.Dispose();
        }
    }
}