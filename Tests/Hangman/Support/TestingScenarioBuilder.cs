using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hangman.Infrastructure;
using Hangman.Core.Models;

namespace Tests.Hangman.Support
{
    public class TestingScenarioBuilder
    {
        private readonly SqlContext _context;

        public TestingScenarioBuilder(SqlContext context)
        {
            _context = context;
        }

        public async Task<List<GameRoom>> BuildScenarioWithThreeRooms(string name1 = "Room 1", string name2 = "Room 2",
            string name3 = "Room 3")
        {
            var gameRooms = new List<GameRoom>()
            {
                new GameRoom {Name = "Game Room 1"},
                new GameRoom {Name = "Game Room 2"},
                new GameRoom {Name = "Game Room 3"}
            };

            await _context.AddRangeAsync(gameRooms);
            await _context.SaveChangesAsync();

            return gameRooms;
        }

        public async Task<Tuple<List<GameRoom>, List<Player>>> BuildScenarioWithThreeRoomsAndThreePlayers()
        {
            var players = new List<Player>()
            {
                new Player {Name = "Player 1"},
                new Player {Name = "Player 2"},
                new Player {Name = "Player 3"}
            };

            await _context.AddRangeAsync(players);
            await _context.SaveChangesAsync();
            var gameRooms = await BuildScenarioWithThreeRooms();

            return Tuple.Create(gameRooms, players);
        }

        public async Task<Tuple<GameRoom, Player, GameRoomPlayer>> BuildScenarioWithAPlayerInRoom(
            bool isInRoom = true, bool isHost = false, bool isBanned = false)
        {
            var player = new Player() { Name = "Player 1" };
            var gameRoom = new GameRoom() { Name = "Game Room 1" };
            var gameRoomPlayer = new GameRoomPlayer()
            {
                GameRoom = gameRoom,
                Player = player,
                IsHost = isHost,
                IsBanned = isBanned,
                IsInRoom = isInRoom,
            };

            await _context.AddAsync(player);
            await _context.AddAsync(gameRoom);
            await _context.AddAsync(gameRoomPlayer);
            await _context.SaveChangesAsync();

            return Tuple.Create(gameRoom, player, gameRoomPlayer);
        }
    }
}