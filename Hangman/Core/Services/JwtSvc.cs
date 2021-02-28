using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Hangman.Core.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Hangman.Core.Services
{
    public interface IJwtSvc
    {
        string GenerateToken(User user);
        ClaimsIdentity CreateUserIdentityClaims(User user);
    }

    public class JwtSvc : IJwtSvc
    {
        private readonly SecretsConfig _secretsConfig;

        public JwtSvc(IOptions<SecretsConfig> secretsConfigOptions)
        {
            _secretsConfig = secretsConfigOptions.Value;
        }

        public string GenerateToken(User user)
        {
            var jwtSignaturePrivateKey = Encoding.ASCII.GetBytes(_secretsConfig.JwtSignaturePrivateKey);
            var jwtSignature = new SigningCredentials(new SymmetricSecurityKey(jwtSignaturePrivateKey),
                SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = CreateUserIdentityClaims(user),
                Expires = DateTime.UtcNow.AddHours(_secretsConfig.JwtExpirationPeriodInHours),
                SigningCredentials = jwtSignature
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public ClaimsIdentity CreateUserIdentityClaims(User user)
        {
            return new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Name, user.Username.ToString()),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            });
        }
    }
}