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
using Hangman.Core.Models;
using System;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Hangman.Core.Services;
using System.Linq;

namespace Tests.Hangman.Integration.Api.V1.Controllers
{
    public class UserControllerTests : WebHostTestCase<Startup>
    {
        private readonly Faker<CreateUserRequestDTO> _fakerCreateUserRequest;
        private readonly string _userLoginEndpointV1;
        private readonly string _userRegisterEndpointV1;

        public UserControllerTests()
        {
            _userRegisterEndpointV1 = "api/v1/user/register";
            _userLoginEndpointV1 = "api/v1/user/login";
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

        [Fact(DisplayName = "Should not register a new user if his username is already taken")]
        public async Task ShouldNotRegisterANewUserIfHisUsernameIsAlreadyTaken()
        {
            // Arrange
            var createUserRequest = _fakerCreateUserRequest.Generate();
            var previousUser = new User
            {
                FirstName = createUserRequest.FirstName,
                LastName = createUserRequest.LastName,
                Username = createUserRequest.Username,
                PasswordDigest = Array.Empty<byte>(),
                PasswordSalt = Array.Empty<byte>()
            };

            await _sqlContext.Users.AddAsync(previousUser);
            await _sqlContext.SaveChangesAsync();

            // Act - request with the same username
            var response = await _webHostHttpClient.PostAsJsonAsync(_userRegisterEndpointV1, createUserRequest);

            // Assert
            var responseAsString = await response.Content.ReadAsStringAsync();
            var responseDeserialized = JsonConvert.DeserializeObject<dynamic>(responseAsString);
            var responseMessage = (string)responseDeserialized.Message;
            var usersInDb = await _sqlContext.Users.CountAsync();

            responseMessage.Should().Be($"Username {createUserRequest.Username} already exists. Try another one, please.");
            usersInDb.Should().Be(1);  // no new user
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact(DisplayName = "Should allow a user to login and generate a jwt token")]
        public async Task ShouldAllowAUserToLoginAndGenerateAJwtToken()
        {
            // Arrange - rearrange test IWebHost with mock Jwt Svc
            ConfigureTestServices(TestServiceCollections.ConfigureServices_MockJwtSvc);

            // Arrange - create an existing user to login
            var createUserRequest = _fakerCreateUserRequest.Generate();
            await _webHostHttpClient.PostAsJsonAsync(_userRegisterEndpointV1, createUserRequest);
            _sqlContext.Users.Count().Should().Be(1);  // just 1 user: the newly created one

            // Arrange - login as the same created user
            var authenticateUserRequest = new AuthenticationRequestDTO
            {
                Username = createUserRequest.Username,
                Password = createUserRequest.Password
            };

            // Arrange - configure jwt svc return
            var mockJwtSvcManager = _testServiceScope.ServiceProvider.GetRequiredService<Mock<IJwtSvc>>();
            var mockJwtSvc = _testServiceScope.ServiceProvider.GetRequiredService<IJwtSvc>();

            mockJwtSvcManager.Setup(jwtSvc =>
                jwtSvc.GenerateToken(It.IsAny<User>()))
                    .Returns("jwtTokenTest");

            // Act
            var response = await _webHostHttpClient.PostAsJsonAsync(_userLoginEndpointV1, authenticateUserRequest);

            // Assert
            var responseAsString = await response.Content.ReadAsStringAsync();
            var deserializedResponse = JsonConvert.DeserializeObject<LoggedUserJwtResponseDTO>(responseAsString);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            deserializedResponse.Username.Should().Be(createUserRequest.Username);
            deserializedResponse.Token.Should().Be("jwtTokenTest");  // mock output response
        }
    }
}