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
using Tests.Hangman.Support.Utils;

namespace Tests.Hangman.Integration.Api.V1.Controllers
{
    public class GameRoomControllerTests : WebHostTestCase<Startup>
    {
        private readonly Faker<CreateGameRoomDTO> _fakerCreateGameRoomDTO;
        private readonly Faker<GameRoom> _fakerGameRoom;
        private readonly Faker<User> _fakerUser;
        private readonly string _gameRoomJoinEndpointV1;
        private readonly string _gameRoomEndpointV1;

        public GameRoomControllerTests() : base(TestServiceCollections.ConfigureServices_FakeAuthHandler)
        {
            // fakers for data
            _fakerCreateGameRoomDTO = new CreateGameRoomRequestFaker();
            _fakerGameRoom = new CreateGameRoomFaker();
            _fakerUser = new CreateUserFaker();

            // endpoints
            _gameRoomJoinEndpointV1 = "api/v1/gameroom/join";
            _gameRoomEndpointV1 = "api/v1/gameroom";

            // Arrange - add mocking security scheme to bypass JWT bearer scheme
            _webHostHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestingScheme");
        }

        [Fact(DisplayName = "Should create and then retrieve a game room by its id")]
        public async Task ShouldCreateAndThenRetrieveAGameRoomByItsId()
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

        [Fact(DisplayName = "Should guarantee a user can join a room")]
        public async Task ShouldGuaranteeUserCanJoinRoom()
        {
            // Arrange - creates a user, game room and game room user
            var gameRoomUser = await GameRoomSeeds
                .GameRoomWithHost(_fakerGameRoom.Generate(), _fakerUser.Generate(), _sqlContext);
            var gameRoom = gameRoomUser.GameRoom;
            var hostUser = gameRoomUser.User;
            var anotherUser = await UserSeeds.User(_fakerUser.Generate(), _sqlContext);

            // Act - join the room
            var response = await _webHostHttpClient.PostAsync(_gameRoomJoinEndpointV1 + $"/{gameRoom.Id}", null);

            // Assert
            var responseAsString = await response.Content.ReadAsStringAsync();
            var responseDeserializedDynamic = JsonConvert.DeserializeObject<dynamic>(responseAsString);  // JOBject

            response.StatusCode.Should().Be(HttpStatusCode.Ok);

            var gameRoomId = (Guid)responseDeserializedDynamic["gameRoomId"];
            var userId = (Guid)responseDeserializedDynamic["userId"];
            var inRoom = (bool)responseDeserializedDynamic["InRoom"];

            gameRoomId.Should().Be(gameRoom.Id);
            userId.Should().Be(anotherUser.Id);  // another user is now in the room
            inRoom.Should().BeTrue();
        }
    }

    public class GameRoomControllerNoFakeAuthTests : WebHostTestCase<Startup>
    {
        private readonly Faker<CreateGameRoomDTO> _fakerCreateGameRoomDTO;
        private readonly string _gameRoomEndpointV1;

        public GameRoomControllerNoFakeAuthTests()
        {
            _fakerCreateGameRoomDTO = new CreateGameRoomRequestFaker();
            _gameRoomEndpointV1 = "api/v1/gameroom";
        }

        [Fact(DisplayName = "Should not create a new gameroom for unauthenticated user")]
        public async Task ShouldNotCreateANewGameRoomForUnauthenticatedUser()
        {
            // Arrange - add new create game room DTO (no mocked security scheme)
            var createGameRoomRequest = _fakerCreateGameRoomDTO.Generate();

            // Act
            var response = await _webHostHttpClient.PostAsJsonAsync(_gameRoomEndpointV1, createGameRoomRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}