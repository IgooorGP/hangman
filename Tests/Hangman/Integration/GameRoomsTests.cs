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
    }
}