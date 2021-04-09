using System.Text;
using Bogus;
using Hangman.Core.DTOs;
using Hangman.Core.Models;

namespace Tests.Hangman.Support
{
    public class CreateUserRequestFaker : Faker<CreateUserRequestDTO>
    {
        public CreateUserRequestFaker()
        {
            RuleFor(u => u.FirstName, f => f.Name.FirstName());
            RuleFor(u => u.LastName, f => f.Name.LastName());
            RuleFor(u => u.Username, (f, u) => f.Internet.UserName(u.FirstName, u.LastName));
            RuleFor(u => u.Password, f => f.Internet.Password(6));
        }
    }

    public class CreateUserFaker : Faker<User>
    {
        public CreateUserFaker(string username = "TestUsername")
        {
            RuleFor(u => u.FirstName, f => f.Name.FirstName());
            RuleFor(u => u.LastName, f => f.Name.LastName());
            RuleFor(u => u.Username, (f, u) => username);
            RuleFor(u => u.PasswordSalt, (f, u) => Encoding.UTF8.GetBytes(u.FirstName));  // simulates byte[]
            RuleFor(u => u.PasswordDigest, (f, u) => Encoding.UTF8.GetBytes(u.LastName));  // simulates byte[]
        }
    }

    public class CreateGameRoomRequestFaker : Faker<CreateGameRoomDTO>
    {
        public CreateGameRoomRequestFaker()
        {
            RuleFor(room => room.Name, faker => $"{faker.Name.FirstName()}'s room");
        }
    }
}