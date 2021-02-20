using System.Collections.Generic;
using System.Linq;
using Hangman.Core.Services;
using Hangman.Core.Business;
using Hangman.Core.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Hangman.Core.Repository;
using Hangman.Core.Repository.Interfaces;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog;
using AutoMapper;
using Hangman.Core.Infrastructure;
using FluentValidation.AspNetCore;

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
            // SQL context
            services.AddDbContext<SqlContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("SqlConnection")));

            // Health-check services
            services.AddHealthChecks()
                .AddNpgSql(Configuration.GetConnectionString("SqlConnection"));

            // Health-check UI
            services.AddHealthChecksUI()
                .AddPostgreSqlStorage(Configuration.GetConnectionString("SqlConnection"));

            // Automapper
            services.AddAutoMapper(typeof(Startup));

            // Application services
            services.AddScoped(typeof(IHangmanRepositoryAsync<>), typeof(HangmanRepositoryAsync<>))
                .AddScoped<IGameRoomServiceAsync, GameRoomServiceAsync>()
                .AddScoped<IPlayerServiceAsync, PlayerServiceAsync>()
                .AddScoped<IHangmanGame, HangmanGame>()
                .AddScoped<IGameRoomSvc, GameRoomSvc>();

            // Misc
            services.AddHttpContextAccessor();

            // MVC features
            services.AddControllers()  // AddMvcCore() + Data annotations + auth pipelines (no Razor view engine though) 
                .AddFluentValidation()  // validations for incoming DTOs
                .AddNewtonsoftJson(options =>
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
        {
            logger.LogInformation("Configuring start up with environment: {EnvironmentName}", env.EnvironmentName);

            // Middleware for Stacktrace pages
            if (env.IsEnvironment("Local"))
                app.UseDeveloperExceptionPage();
            else
                app.UseExceptionHandler("/handler"); // Middleware for sending exceptions to exc controller handler

            // Middleware for condensing many access log lines into a SINGLE useful one
            app.UseSerilogRequestLogging();

            // Middleware for HTTP-to-HTTPS redirection
            app.UseHttpsRedirection();

            // Middleware for request matching/resolution to endpoints
            app.UseRouting();

            // Middleware for authorization (always after UseRouting so request routing is available
            // and before UseEndpoints so users are authorized or not before using endpoints)
            app.UseAuthorization();

            // Middleware for activating the healthcheck UI
            app.UseHealthChecksUI(options => options.UIPath = "/healthcheck-dashboard");

            // Middleware for execution of the requests (decoupled from UseRouting -- bare resolution)
            app.UseEndpoints(endpoints =>
            {
                // changes health-check endpoint to return a JSON rather than plain/text 'Healthy'
                // this endpoint must match the endpoint of 'HealthChecksUI' URI
                endpoints.MapHealthChecks("/healthcheck", new HealthCheckOptions
                {
                    Predicate = _ => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });

                // Controllers execute their requests
                endpoints.MapControllers();
            });

            // Migrations and seeding
            Migrate(app, logger, executeSeedDb: env.IsEnvironment("Local"));

            logger.LogInformation("Host configuration is all done!");
        }

        public static void Migrate(IApplicationBuilder app, ILogger<Startup> logger, bool executeSeedDb = false)
        {
            using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            using var context = serviceScope.ServiceProvider.GetService<SqlContext>();

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
        private static void SeedDb(SqlContext context, ILogger<Startup> logger)
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