using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Hangman.Core.DTOs;
using Hangman.Core.Exceptions;
using Hangman.Infrastructure;
using Hangman.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Hangman.Core.Services
{
    public interface IGameRoomSvc
    {
        public Task<GameRoom> GetById(Guid gameRoomId);
        public Task<GameRoom> Create(CreateGameRoomDTO createGameRoomDTO, string? username);
        public Task<UserInRoomDTO> JoinRoom(Guid gameRoomId, string? username);
        public Task<Tuple<IList<GameRoom>, int>> GetPaginated(SearchGameRoomDTO searchGameRoomDTO);
    }

    public class GameRoomSvc : IGameRoomSvc
    {
        private readonly ILogger<GameRoomSvc> _logger;
        private readonly IUserSvc _userSvc;
        private readonly IMapper _mapper;
        private readonly SqlContext _db;

        public GameRoomSvc(SqlContext db, ILogger<GameRoomSvc> logger, IUserSvc userSvc, IMapper mapper)
        {
            _userSvc = userSvc;
            _logger = logger;
            _mapper = mapper;
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
                .Include(room => room.GameRoomUsers)
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

        public async Task<GameRoom> Create(CreateGameRoomDTO createGameRoomDTO, string? username)
        {
            _logger.LogInformation($"Fetching user with username: {username} ...");
            var user = await _userSvc.GetByUsernameRequired(username);

            _logger.LogInformation("Creating new game room...");
            var newGameRoom = _mapper.Map<CreateGameRoomDTO, GameRoom>(createGameRoomDTO);
            await _db.GameRooms.AddAsync(newGameRoom);

            _logger.LogDebug("Creating new game room user...");
            var gameRoomUser = new GameRoomUser
            {
                User = user,
                UserId = user.Id,
                GameRoomId = newGameRoom.Id,
                GameRoom = newGameRoom,
                IsHost = true,
                IsInRoom = true
            };
            await _db.GameRoomUsers.AddAsync(gameRoomUser);

            _logger.LogDebug("Commiting...");
            await _db.SaveChangesAsync();

            _logger.LogInformation("Returning new game room...");
            return newGameRoom;
        }

        public async Task<UserInRoomDTO> JoinRoom(Guid gameRoomId, string? username)
        {
            _logger.LogInformation($"Fetching user with username: {username} ...");
            var user = await _userSvc.GetByUsernameRequired(username);

            _logger.LogInformation($"Fetching game room: {gameRoomId} ...");
            var gameRoomUser = await _db.GameRoomUsers
                .Where(gameRoomUser => gameRoomUser.GameRoomId == gameRoomId && gameRoomUser.UserId == user.Id)
                .FirstOrDefaultAsync();

            // can only be null here if the game room does not exist (user is validated w/ middleware!)
            if (gameRoomUser is null)
                throw new ObjectDoesNotExist("Game room was not found.");

            // joins the room
            gameRoomUser.IsInRoom = true;

            _db.GameRoomUsers.Update(gameRoomUser);
            await _db.SaveChangesAsync();

            return new UserInRoomDTO
            {
                GameRoomId = gameRoomId,
                UserId = user.Id,
                InRoom = true
            };
        }
    }
}