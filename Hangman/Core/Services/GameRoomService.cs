using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangman.Core.DTOs;
using Hangman.Core.Exceptions;
using Hangman.Core.Infrastructure;
using Hangman.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Hangman.Core.Services
{
    public interface IGameRoomSvc
    {
        public Task<GameRoom> GetById(Guid gameRoomId);
        public Task<Tuple<IList<GameRoom>, int>> GetPaginated(SearchGameRoomDTO searchGameRoomDTO);
    }

    public class GameRoomSvc : IGameRoomSvc
    {
        private readonly ILogger<GameRoomSvc> _logger;
        private readonly SqlContext _db;

        public GameRoomSvc(SqlContext db, ILogger<GameRoomSvc> logger)
        {
            _logger = logger;
            _db = db;
        }

        public async Task<GameRoom> GetById(Guid gameRoomId)
        {
            _logger.LogInformation("Calling gameRoomService to get room with id: {id:l}", gameRoomId);
            var gameRoom = await _db.GameRooms.FindAsync(gameRoomId);

            if (gameRoom is null)
                throw new ObjectDoesNotExist("Game room was not found.");

            return gameRoom;
        }

        public async Task<Tuple<IList<GameRoom>, int>> GetPaginated(SearchGameRoomDTO searchGameRoomDTO)
        {
            _logger.LogInformation("Getting paginated game rooms...");
            var baseQuery = _db.GameRooms
                .Include(room => room.GameRoomPlayers)
                .Include(room => room.GuessWords)
                .OrderBy(room => room.CreatedAt)
                .AsQueryable();

            // Narrows down by name, if supplied
            if (!string.IsNullOrEmpty(searchGameRoomDTO.Name))
                baseQuery = baseQuery.Where(room => room.Name == searchGameRoomDTO.Name);

            var gameRooms = await baseQuery
                .Paginate(searchGameRoomDTO.PageSize, searchGameRoomDTO.PageNumber)
                .ToListAsync() ?? new List<GameRoom>();
            var totalGameRooms = await _db.GameRooms.CountAsync();

            return new Tuple<IList<GameRoom>, int>(gameRooms, totalGameRooms);
        }
    }
}