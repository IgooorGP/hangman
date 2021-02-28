using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangman.Core.DTOs;
using Hangman.Core.Exceptions;
using Hangman.Core.Models;
using Hangman.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hangman.Core.Services
{
    public interface IUserSvc
    {
        public Task<User> Create(CreateUserRequestDTO createUserRequestDTO);
        public Task<User> Authenticate(AuthenticationRequestDTO authenticationRequestDTO);
    }

    public class UserSvc : IUserSvc
    {
        private readonly ILogger<UserSvc> _logger;
        private readonly SqlContext _db;

        public UserSvc(SqlContext db, ILogger<UserSvc> logger)
        {
            _logger = logger;
            _db = db;
        }

        public async Task<User> Authenticate(AuthenticationRequestDTO authenticationRequestDTO)
        {
            var username = authenticationRequestDTO.Username;
            var password = authenticationRequestDTO.Password;

            _logger.LogInformation("User {username} is trying to login...", username);
            var user = await _db.Users.SingleOrDefaultAsync(x => x.Username == username);

            if (user is null)
            {
                _logger.LogInformation("Username was not found...");
                throw new AuthenticationFailure("Username or password is invalid.");
            }

            if (!VerifyPasswordHash(password, user.PasswordSalt, user.PasswordDigest))
            {
                _logger.LogInformation("Password digest did not match...");
                throw new AuthenticationFailure("Username or password is invalid.");
            }

            _logger.LogInformation("User has successfully logged in...");

            return user;
        }

        public async Task<User> Create(CreateUserRequestDTO createUserRequestDTO)
        {
            _logger.LogInformation("Received new create user request {createUserRequestDTO}", createUserRequestDTO);

            var username = createUserRequestDTO.Username;
            var password = createUserRequestDTO.Password;

            if (_db.Users.Any(user => user.Username == username))
                throw new ObjectAlreadyExists($"Username {username} already exists. Try another one, please.");

            var (passwordSalt, passwordDigest) = HashPassword(password);

            var newUser = new User
            {
                FirstName = createUserRequestDTO.FirstName,
                LastName = createUserRequestDTO.LastName,
                Username = createUserRequestDTO.Username,
                PasswordSalt = passwordSalt,
                PasswordDigest = passwordDigest
            };

            await _db.Users.AddAsync(newUser);
            await _db.SaveChangesAsync();

            return newUser;
        }

        public Tuple<byte[], byte[]> HashPassword(string password)
        {
            // creates an hmac authentication code for generating 512 bytes digests
            using var hmac = new System.Security.Cryptography.HMACSHA512();

            var passwordSaltBytes = hmac.Key;
            var passwordDigestBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            return new Tuple<byte[], byte[]>(passwordSaltBytes, passwordDigestBytes);
        }

        public static bool VerifyPasswordHash(string loginAttemptPassword,
            byte[] storedPasswordSaltBytes, byte[] storedPasswordDigestBytes)
        {
            var passwordMatch = true;

            // out of the previously stored salt, re-hash the login attempt password and compare with the previous digest!
            using (var hmac = new System.Security.Cryptography.HMACSHA512(storedPasswordSaltBytes))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginAttemptPassword));

                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedPasswordDigestBytes[i])
                        passwordMatch = false;  // no early breaking to avoid easier guessing
                }
            }

            return passwordMatch;
        }
    }
}