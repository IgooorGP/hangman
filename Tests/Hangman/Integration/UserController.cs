using System.Net.Http;
using System.Threading.Tasks;
using Bogus;
using Hangman;
using Hangman.Core.DTOs;
using Tests.Hangman.Support;
using Xunit;
using FluentAssertions;
using System.Net;
using Microsoft.EntityFrameworkCore;

namespace Tests.Hangman.Integration
{
    public class UserControllerTests : WebHostTestCase<Startup>
    {
        private readonly Faker<CreateUserRequestDTO> _fakerCreateUserRequest;
        private readonly string _userRegisterEndpointV1;

        public UserControllerTests()
        {
            _userRegisterEndpointV1 = "api/v1/user/register";
            _fakerCreateUserRequest = UserFakerFactory.CreateUserRequestDTOFaker();
        }

        [Fact(DisplayName = "Should register a new user")]
        public async Task ShouldRegisterNewUser()
        {
            // Arrange
            var createUserRequest = _fakerCreateUserRequest.Generate();

            // Act
            var response = await _webHostHttpClient.PostAsJsonAsync(_userRegisterEndpointV1, createUserRequest);

            // Assert            
            var createdUser = await _sqlContext.Users.FirstOrDefaultAsync();

            response.StatusCode.Should().Be(HttpStatusCode.Created);

            createdUser.Should().NotBeNull();
            createdUser.FirstName.Should().Be(createUserRequest.FirstName);
            createdUser.LastName.Should().Be(createUserRequest.LastName);
            createdUser.Username.Should().Be(createUserRequest.Username);
        }
    }
}