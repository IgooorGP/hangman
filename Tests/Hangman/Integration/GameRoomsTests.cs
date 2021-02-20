using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tests.Hangman.Support;
using Xunit;


namespace Tests.Hangman.Integration
{
    public class GameRoomsTests : TestingCaseFixture<TestingStartUp>
    {
        [Theory(DisplayName = "Should make a request to /api/v1/gameroom and get all 3 rooms")]
        [InlineData("/api/v1/gameroom")]
        public async Task TestShouldMakeRequestToGameRoomAndGetHttp200(string url)
        {
            // arrange
            var testingScenarioBuilder = new TestingScenarioBuilder(DbContext);
            await testingScenarioBuilder.BuildScenarioWithThreeRooms();

            // act
            var response = await Client.GetAsync(url);
            var responseAsString = await response.Content.ReadAsStringAsync();
            var responseAsJson = JArray.Parse(responseAsString); // JObject if response is not an array

            // assert
            Assert.Equal(3, responseAsJson.Count);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Theory(DisplayName = "Should create a new room and get it with its id")]
        [InlineData("/api/v1/gameroom")]
        public async Task TestShouldMakeRequestToGameRoomAndGetHttp200AfterCreation(string url)
        {
            // arrange
            var postData = JsonConvert.SerializeObject(new
            {
                Name = "Game Room Test"
            });
            var payload = new StringContent(postData, Encoding.UTF8, "application/json");

            // act
            var postResponse = await Client.PostAsync(url, payload);
            var postResponseAsJson = JObject.Parse(await postResponse.Content.ReadAsStringAsync());
            var createdGameRoomId = postResponseAsJson["id"].ToString();
            var createdGameRoomName = postResponseAsJson["name"].ToString();

            // assert
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
            Assert.Equal("Game Room Test", createdGameRoomName); // expected game room

            var getResponse = await Client.GetAsync(url + $"/{createdGameRoomId}");
            var getResponseAsJson = JObject.Parse(await getResponse.Content.ReadAsStringAsync());

            Assert.Equal(createdGameRoomId, getResponseAsJson["id"]);
            Assert.Equal(createdGameRoomName, getResponseAsJson["name"]);
        }

        [Theory(DisplayName = "Should make a player join a room successfully")]
        [InlineData("/api/v1/gameroom")]
        public async Task TestPlayerShouldJoinRoomWithoutBeingHost(string url)
        {
            // arrange
            var testingScenarioBuilder = new TestingScenarioBuilder(DbContext);
            var (rooms, players) = await testingScenarioBuilder.BuildScenarioWithThreeRoomsAndThreePlayers();  // deconstruction

            var gameRoomOne = rooms[0];
            var playerOne = players[0];

            var postData = JsonConvert.SerializeObject(new
            {
                PlayerId = playerOne.Id
            });
            var payload = new StringContent(postData, Encoding.UTF8, "application/json");

            // act
            var gameRoomUrl = url + $"/{gameRoomOne.Id}/join";
            var postResponse = await Client.PostAsync(gameRoomUrl, payload);

            // assert
            var gameRoomPlayer = await DbContext.GameRoomPlayers.FirstAsync();
            var postResponseAsJson = JObject.Parse(await postResponse.Content.ReadAsStringAsync());

            // assert - response
            Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);
            Assert.Equal(gameRoomOne.Id.ToString(), postResponseAsJson["gameRoomId"]);
            Assert.Equal(playerOne.Id.ToString(), postResponseAsJson["playerId"]);
            Assert.True((bool)postResponseAsJson["isInRoom"]);

            // assert - database
            Assert.Equal(1, await DbContext.GameRoomPlayers.CountAsync());
            Assert.Equal(gameRoomPlayer.GameRoomId, gameRoomOne.Id);
            Assert.Equal(gameRoomPlayer.PlayerId, playerOne.Id);
            Assert.False(gameRoomPlayer.IsHost);
            Assert.False(gameRoomPlayer.IsBanned);
            Assert.True(gameRoomPlayer.IsInRoom);  // in room, but not host
        }

        [Theory(DisplayName = "Should make a player rejoin a room that he had left")]
        [InlineData("/api/v1/gameroom")]
        public async Task TestPlayerShouldRejoinRoom(string url)
        {
            // arrange
            var testingScenarioBuilder = new TestingScenarioBuilder(DbContext);
            var (gameRoom, player, _) = await testingScenarioBuilder.BuildScenarioWithAPlayerInRoom(isInRoom: false);  // deconstruction

            var gameRoomPlayer = await DbContext.GameRoomPlayers.FindAsync(gameRoom.Id, player.Id);
            Assert.False(gameRoomPlayer.IsInRoom);  // player is NOT in the room 

            var postData = JsonConvert.SerializeObject(new
            {
                PlayerId = player.Id
            });
            var payload = new StringContent(postData, Encoding.UTF8, "application/json");

            // act
            var gameRoomUrl = url + $"/{gameRoom.Id}/join";
            var postResponse = await Client.PostAsync(gameRoomUrl, payload);

            // assert
            var gameRoomPlayerFromDb = await DbContext.GameRoomPlayers.FirstAsync();
            var postResponseAsJson = JObject.Parse(await postResponse.Content.ReadAsStringAsync());

            // assert - response
            Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);
            Assert.Equal(gameRoom.Id.ToString(), postResponseAsJson["gameRoomId"]);
            Assert.Equal(player.Id.ToString(), postResponseAsJson["playerId"]);
            Assert.True((bool)postResponseAsJson["isInRoom"]);

            // assert - database
            Assert.Equal(1, await DbContext.GameRoomPlayers.CountAsync());
            Assert.Equal(gameRoomPlayerFromDb.GameRoomId, gameRoom.Id);
            Assert.Equal(gameRoomPlayerFromDb.PlayerId, player.Id);
            Assert.False(gameRoomPlayerFromDb.IsHost);
            Assert.False(gameRoomPlayerFromDb.IsBanned);
            Assert.True(gameRoomPlayerFromDb.IsInRoom);  // player IS in room once again!
        }

        [Theory(DisplayName = "Should make a player leave a room that he had joined")]
        [InlineData("/api/v1/gameroom")]
        public async Task TestPlayerShouldLeaveRoom(string url)
        {
            // arrange
            var testingScenarioBuilder = new TestingScenarioBuilder(DbContext);
            var (gameRoom, player, _) = await testingScenarioBuilder.BuildScenarioWithAPlayerInRoom(isInRoom: true);  // deconstruction

            var gameRoomPlayer = await DbContext.GameRoomPlayers.FindAsync(gameRoom.Id, player.Id);
            Assert.True(gameRoomPlayer.IsInRoom);  // player IS in the room 

            var postData = JsonConvert.SerializeObject(new
            {
                PlayerId = player.Id
            });
            var payload = new StringContent(postData, Encoding.UTF8, "application/json");

            // act
            var gameRoomUrl = url + $"/{gameRoom.Id}/leave";
            var postResponse = await Client.PostAsync(gameRoomUrl, payload);

            // assert
            var gameRoomPlayerFromDb = await DbContext.GameRoomPlayers.FirstAsync();
            var postResponseAsJson = JObject.Parse(await postResponse.Content.ReadAsStringAsync());

            // assert - response
            Assert.Equal(HttpStatusCode.OK, postResponse.StatusCode);
            Assert.Equal(gameRoom.Id.ToString(), postResponseAsJson["gameRoomId"]);
            Assert.Equal(player.Id.ToString(), postResponseAsJson["playerId"]);
            Assert.False((bool)postResponseAsJson["isInRoom"]);

            // assert - database
            Assert.Equal(1, await DbContext.GameRoomPlayers.CountAsync());
            Assert.Equal(gameRoomPlayerFromDb.GameRoomId, gameRoom.Id);
            Assert.Equal(gameRoomPlayerFromDb.PlayerId, player.Id);
            Assert.False(gameRoomPlayerFromDb.IsHost);
            Assert.False(gameRoomPlayerFromDb.IsBanned);
            Assert.False(gameRoomPlayerFromDb.IsInRoom);  // player IS NOT in the room anymore -- he has just left!
        }
    }
}