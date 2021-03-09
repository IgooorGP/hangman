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

namespace Tests.Hangman.Integration.Api.V1.Controllers
{
    public class GameRoomControllerTests : WebHostTestCase<Startup>
    {
        private readonly Faker<CreateGameRoomDTO> _fakerCreateGameRoomDTO;
        private readonly string _gameRoomEndpointV1;

        public GameRoomControllerTests() : base(TestServiceCollections.ConfigureServices_FakeAuthHandler)
        {
            _gameRoomEndpointV1 = "api/v1/gameroom";
            _fakerCreateGameRoomDTO = GameRoomFactory.CreateGameRoomDTOFaker();
        }

        [Fact(DisplayName = "Should create and then retrieve a game room by its id")]
        public async Task ShoulsCreateAndThenRetrieveAGameRoomByItsId()
        {
            // Arrange
            var createGameRoomRequest = _fakerCreateGameRoomDTO.Generate();
            _webHostHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestingScheme");

            // Act
            var response = await _webHostHttpClient.PostAsJsonAsync(_gameRoomEndpointV1, createGameRoomRequest);

            // Assert
            var responseAsString = await response.Content.ReadAsStringAsync();
            var responseDeserializedDynamic = JsonConvert.DeserializeObject<dynamic>(responseAsString);  // JOBject

            var gameRoomId = (Guid)responseDeserializedDynamic["id"];
            var createdGameRoom = await _sqlContext.GameRooms.FirstOrDefaultAsync();

            response.StatusCode.Should().Be(HttpStatusCode.Created);
            createdGameRoom.Should().NotBeNull();
            gameRoomId.Should().Be(createdGameRoom.Id.ToString());
        }
    }
}