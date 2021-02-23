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

        public async Task<List<GameRoom>> BuildScenarioWithThreeRooms(string name1 = "Room 1", string name2 = "Room 2", string name3 = "Room 3")
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
    }
}