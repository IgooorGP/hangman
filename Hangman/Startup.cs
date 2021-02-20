using System.Collections.Generic;
using System.Linq;
using Hangman.Services;
using Hangman.Business;
using Hangman.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Hangman.Repository;
using Hangman.Repository.Interfaces;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog;
using AutoMapper;

namespace Hangman
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // injection of health-checking services (database, cache, etc. goes here)
            services.AddHealthChecks()
                .AddNpgSql(Configuration.GetConnectionString("SqlConnection"));

            // injection of the friendly health-check UI service (requires DB for health history persistence)
            // configuration of endpoints for the UI client on the 'HealthChecksUI' of appsettings.json
            services.AddHealthChecksUI()
                .AddPostgreSqlStorage(Configuration.GetConnectionString("SqlConnection"));

            // injection os other services
            services.AddHttpContextAccessor()
                .AddDbContext<HangmanDbContext>(options =>
                    options.UseNpgsql(Configuration.GetConnectionString("SqlConnection")))
                .AddScoped(typeof(IHangmanRepositoryAsync<>), typeof(HangmanRepositoryAsync<>)) // generic repository
                .AddScoped<IGameRoomServiceAsync, GameRoomServiceAsync>()
                .AddScoped<IPlayerServiceAsync, PlayerServiceAsync>()
                .AddScoped<IHangmanGame, HangmanGame>()
                .AddAutoMapper(typeof(Startup))
                .AddControllers()
                .AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);  // ignore loops when serializing JSON
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            logger.LogInformation("Configuring start up with environment: {EnvironmentName}", env.EnvironmentName);

            // dev exceptionss
            if (env.IsDevelopment() || env.IsEnvironment("Local")) app.UseDeveloperExceptionPage();

            // middleware for condensing many access log lines into a SINGLE useful one
            app.UseSerilogRequestLogging();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();

            // middleware for activating the healthcheck UI
            app.UseHealthChecksUI(options => options.UIPath = "/healthcheck-dashboard");

            app.UseEndpoints(endpoints =>
            {
                // changes health-check endpoint to return a JSON rather than plain/text 'Healthy'
                // this endpoint must match the endpoint of 'HealthChecksUI' URI
                endpoints.MapHealthChecks("/healthcheck", new HealthCheckOptions
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });

                endpoints.MapControllers();
            });

            // Migrations and seeding for running locally
            Migrate(app, logger, executeSeedDb: env.IsEnvironment("Local"));

            logger.LogInformation("Host configuration is all done!");
        }

        public static void Migrate(IApplicationBuilder app, ILogger<Startup> logger, bool executeSeedDb = false)
        {
            using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            using var context = serviceScope.ServiceProvider.GetService<HangmanDbContext>();

            // always execute possible missing migrations
            if (context.Database.GetPendingMigrations().ToList().Any())
            {
                logger.LogInformation("Applying migrations...");
                context.Database.Migrate();
            }

            // seeding DB only when asked
            if (!executeSeedDb) return;

            logger.LogInformation("Seeding the database...");
            SeedDb(context, logger);
        }

        /**
         * Seeds DB with pre-defined entities/models.
         */
        private static void SeedDb(HangmanDbContext context, ILogger<Startup> logger)
        {
            if (context.GameRooms.Any())
            {
                logger.LogInformation("Database has already been seeded. Skipping it...");
                return;
            }

            logger.LogInformation("Saving entities...");
            var gameRooms = new List<GameRoom>
            {
                new GameRoom {Name = "Game Room 1"},
                new GameRoom {Name = "Game Room 2"},
                new GameRoom {Name = "Game Room 3"}
            };
            context.AddRange(gameRooms);

            logger.LogInformation("Database has been seeded successfully.");
            context.SaveChanges();
        }
    }
}