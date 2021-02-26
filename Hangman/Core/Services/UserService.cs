using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangman.Core.DTOs;
using Hangman.Core.Exceptions;
using Hangman.Core.Models;
using Hangman.Infrastructure;
using Microsoft.Extensions.Logging;

namespace Hangman.Core.Services
{
    public interface IUserSvc
    {
        public Task<User> Create(CreateUserRequestDTO createUserRequestDTO);
        public User Authenticate(string username, string password);
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

        public User Authenticate(string username, string password)
        {
            _logger.LogInformation("User {username} is trying to login...", username);
            var user = _db.Users.SingleOrDefault(x => x.Username == username);

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
            var username = createUserRequestDTO.Username;
            var password = createUserRequestDTO.Password;

            if (_db.Users.Any(user => x.Username == user.Username))
                throw new Exception("Username " + username + "is already taken");

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

        public Tuple<string, string> HashPassword(string password)
        {
            // creates an hmac authentication code for generating 512 bytes digests
            using var hmac = new System.Security.Cryptography.HMACSHA512();

            var passwordSalt = Encoding.UTF8.GetString(hmac.Key);
            var passwordDigest = Encoding.UTF8.GetString(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));

            return new Tuple<string, string>(passwordSalt, passwordDigest);
        }

        public static bool VerifyPasswordHash(string loginAttemptPassword, string storedPasswordSalt, string storedPasswordDigest)
        {
            var storedPasswordSaltBytes = Encoding.UTF8.GetBytes(storedPasswordSalt);
            var storedPasswordDigestBytes = Encoding.UTF8.GetBytes(storedPasswordDigest);
            var passwordMatch = true;

            if (loginAttemptPassword == null)
                throw new ArgumentNullException("loginAttemptPassword");
            if (string.IsNullOrWhiteSpace(loginAttemptPassword))
                throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");
            if (storedPasswordSaltBytes.Length != 128)
                throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "passwordHash");
            if (storedPasswordSaltBytes.Length != 64)
                throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "passwordHash");

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