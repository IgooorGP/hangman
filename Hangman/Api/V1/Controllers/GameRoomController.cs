using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hangman.Core.Services;
using Hangman.Core.DTOs;
using Hangman.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Hangman.Api.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Hangman.Api.V1.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class GameRoomController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<GameRoomController> _logger;
        private readonly IGameRoomSvc _gameRoomSvc;
        private readonly IUserSvc _userSvc;
        private readonly IMapper _mapper;

        public GameRoomController(IGameRoomSvc gameRoomSvc,
            IUserSvc userSvc,
            ILogger<GameRoomController> logger,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper)
        {
            _httpContextAccessor = httpContextAccessor;
            _gameRoomSvc = gameRoomSvc;
            _userSvc = userSvc;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("{gameRoomId}")]
        public async Task<ActionResult> GetById([FromRoute] Guid gameRoomId)
        {
            var gameRoom = await _gameRoomSvc.GetById(gameRoomId);
            var gameRoomResponse = _mapper.Map<GameRoom, GameRoomResponseDTO>(gameRoom);

            return Ok(gameRoomResponse);
        }

        [HttpGet]
        public async Task<ActionResult> All([FromQuery] SearchGameRoomDTO searchGameRoomDTO)
        {
            var (gameRooms, totalGameRooms) = await _gameRoomSvc.GetPaginated(searchGameRoomDTO);
            var gameRoomsResponse = _mapper.Map<IList<GameRoom>, IList<GameRoomResponseDTO>>(gameRooms);

            return Ok(new PaginatedResponse(gameRoomsResponse, gameRoomsResponse.Count, totalGameRooms));
        }

        [HttpPost]
        public async Task<ActionResult> Create(CreateGameRoomDTO createGameRoomDTO)
        {
            var username = User.Identity.Name;

            var createdGameRoom = await _gameRoomSvc.Create(createGameRoomDTO, username);
            var gameRoomResponse = _mapper.Map<GameRoom, GameRoomResponseDTO>(createdGameRoom);

            return CreatedAtAction(nameof(GetById), new { gameRoomId = gameRoomResponse.Id }, gameRoomResponse);
        }

        [HttpPost]
        [Route("join/{gameRoomId}")]
        public async Task<ActionResult> JoinRoom([FromRoute] Guid gameRoomId)
        {
            var username = User.Identity.Name;
            var userInRoomDTO = await _gameRoomSvc.JoinRoom(gameRoomId, username);

            return Ok(userInRoomDTO);
        }
    }
}