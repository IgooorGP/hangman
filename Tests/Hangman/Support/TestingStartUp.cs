using System.Linq;
using Hangman;
using Hangman.Application;
using Hangman.Repository;
using Hangman.Repository.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Tests.Hangman.Support
{
    /**
     * Testing start up classes that are used to injected mocked services and migrate
     * the database for the integration tests.
     */
    public class TestingStartUp
    {
        public TestingStartUp(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        
        public void ConfigureServices(IServiceCollection services)
        {
            // this is required to add the controllers of the main Hangman project
            var startupAssembly = typeof(Startup).Assembly;

            services.AddHttpContextAccessor()
                .AddDbContext<HangmanDbContext>(options =>
                    options.UseNpgsql(Configuration.GetConnectionString("DBConnection")), ServiceLifetime.Singleton)
                .AddScoped(typeof(IHangmanRepositoryAsync<>), typeof(HangmanRepositoryAsync<>)) // generic repository
                .AddScoped<IGameRoomServiceAsync, GameRoomServiceAsync>()
                .AddScoped<IPlayerServiceAsync, PlayerServiceAsync>()
                .AddControllers()
                .AddApplicationPart(startupAssembly) // adds controllers from main project
                .AddControllersAsServices();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment() || env.IsEnvironment("Local"))
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            // middleware for condensing many access log lines into a SINGLE useful one
            app.UseSerilogRequestLogging();

            app.UseRouting();

            app.UseAuthorization();

            // middleware for activating the health check UI
            app.UseHealthChecksUI(options => options.UIPath = "/healthcheck-dashboard");
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            
            Migrate();
        }

        /**
         * Applies possible missing migrations from the database.
         */
        private void Migrate()
        {
            // testing migrations
            var dbConnectionString = Configuration.GetConnectionString("DBConnection");
            var options = new DbContextOptionsBuilder<HangmanDbContext>()
                .UseNpgsql(dbConnectionString)
                .Options;

            var context = new HangmanDbContext(options);
            
            // always execute possible missing migrations
            if (!context.Database.GetPendingMigrations().ToList().Any()) return;
            context.Database.Migrate();
        }
    }
}