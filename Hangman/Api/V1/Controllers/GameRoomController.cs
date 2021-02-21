using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangman.Core.Services;
using Hangman.Core.DTOs;
using Hangman.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Hangman.Core.Infrastructure;
using AutoMapper;
using Hangman.Api.Pagination;

namespace Hangman.Api.V1.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class GameRoomController : ControllerBase
    {
        private readonly IGameRoomServiceAsync _gameRoomServiceAsync;
        private readonly IPlayerServiceAsync _playerServiceAsync;
        private readonly ILogger<GameRoomController> _logger;
        private readonly IGameRoomSvc _gameRoomSvc;
        private readonly IMapper _mapper;

        public GameRoomController(IGameRoomSvc gameRoomSvc,
            IGameRoomServiceAsync gameRoomServiceAsync,
            IPlayerServiceAsync playerServiceAsync,
            ILogger<GameRoomController> logger,
            IMapper mapper)
        {
            _gameRoomServiceAsync = gameRoomServiceAsync;
            _playerServiceAsync = playerServiceAsync;
            _gameRoomSvc = gameRoomSvc;
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
            var createdGameRoom = await _gameRoomSvc.Create(createGameRoomDTO);
            var gameRoomResponse = _mapper.Map<GameRoom, GameRoomResponseDTO>(createdGameRoom);

            return CreatedAtAction(nameof(GetById), new { gameRoomId = gameRoomResponse.Id }, gameRoomResponse);
        }

        [HttpPost]
        [Route("{gameRoomId}/join")]
        public async Task<ActionResult<PlayerInRoomDTO>> JoinRoom(Guid gameRoomId, PlayerDTO playerDTO)
        {
            var joinRoomDTO = new JoinRoomDTO { GameRoomId = gameRoomId, PlayerId = playerDTO.PlayerId, IsHost = false };
            _logger.LogInformation("Start join room validation: {@joinRoomDTO}", joinRoomDTO);

            var validator = new GameRoomPlayerValidator(_gameRoomServiceAsync, _playerServiceAsync);
            var validationResult = validator.Validate(joinRoomDTO);

            if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

            _logger.LogInformation("Validations were successfull, adding player to the room...");
            var playerInRoomDTO = await _gameRoomServiceAsync.JoinRoom(joinRoomDTO);

            return StatusCode(200, playerInRoomDTO);
        }

        [HttpPost]
        [Route("{gameRoomId}/leave")]
        public async Task<ActionResult<PlayerInRoomDTO>> LeaveRoom(Guid gameRoomId, PlayerDTO playerDTO)
        {
            var leaveRoomDTO = new LeaveRoomDTO { GameRoomId = gameRoomId, PlayerId = playerDTO.PlayerId };
            _logger.LogInformation("Leave room data received: {@leaveRoomDTO}", leaveRoomDTO);

            var validator = new PlayerPreviouslyInRoomValidator(_gameRoomServiceAsync, _playerServiceAsync);
            var validationResult = validator.Validate(leaveRoomDTO);

            if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

            _logger.LogInformation("Validations were successfull, removing player from the room...");
            var playerInRoomDTO = await _gameRoomServiceAsync.LeaveRoom(leaveRoomDTO);

            return StatusCode(200, playerInRoomDTO);
        }

        [HttpGet]
        [Route("{gameRoomId}/guessword")]
        public async Task<ActionResult<IEnumerable<GuessWord>>> GetGuessWordsInRoom(Guid gameRoomId)
        {
            _logger.LogInformation("Getting all guessed words for room {:l}", gameRoomId);
            var guessedWords = await _gameRoomServiceAsync.GetAllGuessedWords(gameRoomId);

            return Ok(guessedWords);
        }

        [HttpPost]
        [Route("{gameRoomId}/guessword")]
        public async Task<ActionResult<GuessWord>> CreateGuessWordInRoom(Guid gameRoomId, GuessWordRequestDTO guessWordRequestDTO)
        {
            _logger.LogInformation("New guess word creation: {@guessWordRequestDTO}", guessWordRequestDTO);
            var guessWordDTO = new GuessWordDTO
            {
                GameRoomId = gameRoomId,
                PlayerId = guessWordRequestDTO.PlayerId,
                GuessWord = guessWordRequestDTO.GuessWord
            };

            var validator = new GuessWordCreationHostValidation(_gameRoomServiceAsync, _playerServiceAsync);
            var validationResult = validator.Validate(guessWordDTO);

            if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

            _logger.LogInformation("Validations were successfull, removing player from the room...");
            var guessWordResponseDTO = await _gameRoomServiceAsync.CreateGuessWord(guessWordDTO);

            return StatusCode(201, guessWordResponseDTO);
        }

        [HttpGet]
        [Route("{gameRoomId}/guessword/{guessWordId}")]
        public async Task<ActionResult<GameStateDTO>> GetGuessWord(Guid gameRoomId, Guid guessWordId)
        {
            var guessWordInGuessRoomDTO = new GuessWordInGuessRoomDTO { GameRoomId = gameRoomId, GuessWordId = guessWordId };
            _logger.LogInformation("Getting game state for: {@guessWordInGuessRoomDTO", guessWordInGuessRoomDTO);

            var validator = new GuessWordInGameRoomValidator(_gameRoomServiceAsync, _playerServiceAsync);
            var validationResult = validator.Validate(guessWordInGuessRoomDTO);

            if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

            var guessWord = await _gameRoomServiceAsync.GetGuessedWord(guessWordId);
            var gameRound = guessWord!.Round;  // previously validated
            var guessWordIfRoundIsOver = gameRound.IsOver ? guessWord.Word : null;

            return Ok(new GameStateDTO
            {
                GuessWord = guessWordIfRoundIsOver,
                IsOver = gameRound.IsOver,
                PlayerHealth = guessWord.Round.Health,
                GuessWordSoFar = _gameRoomServiceAsync.GetGuessWordStateSoFar(guessWord),
                GuessedLetters = guessWord.GuessLetters.Select(letter => letter.Letter),
            });
        }

        [HttpPost]
        [Route("{gameRoomId}/guessword/{guessWordId}/guessletter")]
        public async Task<ActionResult<GameStateDTO>> CreateGuessWord(Guid gameRoomId, Guid guessWordId,
            NewGuessLetterRequestDTO newGuessLetterRequestDTO)
        {
            var newGuessLetterDTO = new NewGuessLetterDTO
            {
                GameRoomId = gameRoomId,
                GuessWordId = guessWordId,
                PlayerId = newGuessLetterRequestDTO.PlayerId,
                GuessLetter = newGuessLetterRequestDTO.GuessLetter
            };
            _logger.LogInformation("New guess letter creation: {@newGuessLetterDTO", newGuessLetterDTO);

            var validator = new NewGuessLetterValidator(_gameRoomServiceAsync, _playerServiceAsync);
            var validationResult = validator.Validate(newGuessLetterDTO);

            if (!validationResult.IsValid) return BadRequest(validationResult.Errors);

            var gameStateDTO = await _gameRoomServiceAsync.UpdateGameRoundState(newGuessLetterDTO);
            return StatusCode(201, gameStateDTO);
        }
    }
}