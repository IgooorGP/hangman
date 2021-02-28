using Bogus;
using Hangman.Core.DTOs;

namespace Tests.Hangman.Support
{
    public static class UserFakerFactory
    {
        public static Faker<CreateUserRequestDTO> CreateUserRequestDTOFaker()
        {
            return new CreateUserRequestFaker();
        }
    }

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
}