using System.Text;
using Hangman.Core.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Hangman.Infrastructure.Security
{
    public static class JwtSecurityExtensions
    {
        public static void AddJwtAuthentication(this IServiceCollection services, string JwtSignaturePrivateKey)
        {
            var privateKey = Encoding.ASCII.GetBytes(JwtSignaturePrivateKey);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(privateKey),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
                // jwt bearer events to handle authentication
                options.Events = new JwtBearerEvents
                {
                    // After jwt signature validation and ClaimsIdentity has been generated
                    OnTokenValidated = async (context) =>
                    {
                        var userSvc = context.HttpContext.RequestServices.GetRequiredService<IUserSvc>();
                        var username = context.Principal.Identity.Name;

                        if (username is null)
                        {
                            context.Fail("Unauthorized");
                            return;
                        }

                        var user = await userSvc.GetByUsername(username);

                        if (user is null)
                            context.Fail("Unauthorized");
                    }
                };
            });
        }
    }
}