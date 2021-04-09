using System.Net.Http;
using System.Threading.Tasks;
using Bogus;
using Hangman;
using Hangman.Core.DTOs;
using Newtonsoft.Json;
using Tests.Hangman.Support;
using Xunit;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System;
using Hangman.Core.Models;
using System.Linq;

namespace Tests.Hangman.Integration.Api.V1.Controllers
{
    public class GameRoomControllerTests : WebHostTestCase<Startup>
    {
        private readonly Faker<CreateGameRoomDTO> _fakerCreateGameRoomDTO;
        private readonly Faker<User> _fakerUser;
        private readonly string _gameRoomEndpointV1;

        public GameRoomControllerTests() : base(TestServiceCollections.ConfigureServices_FakeAuthHandler)
        {
            _fakerCreateGameRoomDTO = new CreateGameRoomRequestFaker();
            _fakerUser = new CreateUserFaker();
            _gameRoomEndpointV1 = "api/v1/gameroom";

            // Arrange - add mocking security scheme
            _webHostHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestingScheme");
        }

        [Fact(DisplayName = "Should create and then retrieve a game room by its id")]
        public async Task ShoulsCreateAndThenRetrieveAGameRoomByItsId()
        {
            // Arrange - add mock user -> will require a possible new scheme!
            var user = _fakerUser.Generate();

            _sqlContext.Users.Add(user);
            await _sqlContext.SaveChangesAsync();

            // Arrange - add new create game room DTO
            var createGameRoomRequest = _fakerCreateGameRoomDTO.Generate();

            // Act
            var response = await _webHostHttpClient.PostAsJsonAsync(_gameRoomEndpointV1, createGameRoomRequest);

            // Assert - Status code
            var responseAsString = await response.Content.ReadAsStringAsync();
            var responseDeserializedDynamic = JsonConvert.DeserializeObject<dynamic>(responseAsString);  // JOBject

            response.StatusCode.Should().Be(HttpStatusCode.Created);

            // Assert - db state
            var gameRoomId = (Guid)responseDeserializedDynamic["id"];
            var createdGameRoom = await _sqlContext.GameRooms.FirstOrDefaultAsync();

            // Game room itself
            createdGameRoom.Should().NotBeNull();
            gameRoomId.Should().Be(createdGameRoom.Id.ToString());
            createdGameRoom.Name.Should().Be(createGameRoomRequest.Name);

            // Associated user
            createdGameRoom.GameRoomUsers.First().User.FirstName.Should().Be(user.FirstName);
            createdGameRoom.GameRoomUsers.First().User.LastName.Should().Be(user.LastName);
            createdGameRoom.GameRoomUsers.First().User.Username.Should().Be(user.Username);
            createdGameRoom.GameRoomUsers.First().IsHost.Should().BeTrue();
        }
    }
}