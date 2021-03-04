using System;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Hangman.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Tests.Hangman.Support
{
    public class WebHostTestCase<TStartup> : IDisposable where TStartup : class
    {
        protected IDbContextTransaction _sqlContextTransaction;
        protected SqlContext _sqlContext;
        protected IServiceScope _testServiceScope;
        protected HttpClient _webHostHttpClient;

        public WebHostTestCase(Action<IServiceCollection> configureServices = null)
        {
            BeforeEachTest(configureServices);
        }

        public void Dispose()
        {
            AfterEachTest();
        }

        private void BeforeEachTest(Action<IServiceCollection> configureServices)
        {
            configureServices ??= TestInjections.DefaultConfiguration;
            var factory = new WebHostFactory<TStartup>(configureServices);

            _webHostHttpClient = factory.CreateClient();
            _testServiceScope = factory.Server.Services.CreateScope();

            _sqlContext = _testServiceScope.ServiceProvider.GetRequiredService<SqlContext>();
            _sqlContext.Database.Migrate();
            _sqlContextTransaction = _sqlContext.Database.BeginTransaction();
        }

        private void AfterEachTest()
        {
            _sqlContextTransaction?.Rollback();
            _webHostHttpClient.Dispose();
            _testServiceScope.Dispose();
        }

        public void ConfigureTestServices(Action<IServiceCollection> newConfigureServices)
        {
            AfterEachTest(); // dispose of previously created services

            var newFactory = new WebHostFactory<TStartup>(newConfigureServices);

            // recreate services from new factory
            _webHostHttpClient = newFactory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
            _testServiceScope = newFactory.Server.Services.CreateScope();

            _sqlContext = _testServiceScope.ServiceProvider.GetRequiredService<SqlContext>();
            _sqlContextTransaction = _sqlContext.Database.BeginTransaction();
        }
    }
}