using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Hangman.Core.Models;
using Microsoft.IdentityModel.Tokens;

namespace Hangman.Core.Services
{
    public class TokenService
    {
        private readonly SecretsConfig _secretsConfig;

        public TokenService(SecretsConfig secretsConfig)
        {
            _secretsConfig = secretsConfig;
        }

        public string GenerateToken(User user)
        {
            var key = Encoding.ASCII.GetBytes(_secretsConfig.JwtSignaturePrivateKey);

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Username.ToString()),
                    new Claim(ClaimTypes.Role, user.Role.ToString())
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}