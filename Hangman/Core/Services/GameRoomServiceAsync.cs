using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Hangman.Core.Business;
using Hangman.Core.DTOs;
using Hangman.Core.Models;
using Hangman.Core.Repository.Interfaces;
using Microsoft.Extensions.Logging;

namespace Hangman.Core.Services
{
    public class GameStateData
    {
        public string? GuessWord;
        public IEnumerable<string> GuessWordSoFar = null!;
        public IEnumerable<string> GuessedLetters = null!;
        public bool IsOver;
        public int PlayerHealth;
    }

    public class NewGuessLetterData
    {
        [Required] public string GuessLetter { get; set; } = default!; // null-forgiving as this property is required
        [Required] public string PlayerName { get; set; } = default!; // null-forgiving as this property is required
    }

    /**
     * Application service that is used to perform CRUD operations over the entity
     * GameRoom.
     */
    public class GameRoomServiceAsync : IGameRoomServiceAsync
    {
        private readonly IHangmanRepositoryAsync<GameRoomPlayer> _repositoryGameRoomPlayer;
        private readonly IHangmanRepositoryAsync<GuessLetter> _repositoryGuessLetter;
        private readonly IHangmanRepositoryAsync<GameRound> _repositoryGameRound;
        private readonly IHangmanRepositoryAsync<GuessWord> _repositoryGuessWord;
        private readonly IHangmanRepositoryAsync<Player> _repositoryPlayer;
        private readonly IHangmanRepositoryAsync<GameRoom> _repository;
        private readonly ILogger<GameRoomServiceAsync> _logger;
        private readonly IHangmanGame _gameLogic;
        private readonly IMapper _mapper;

        public GameRoomServiceAsync(IHangmanRepositoryAsync<GameRoom> repository,
            IHangmanRepositoryAsync<GameRoomPlayer> repositoryGameRoomPlayer,
            IHangmanRepositoryAsync<GuessLetter> repositoryGuessLetter,
            IHangmanRepositoryAsync<GuessWord> repositoryGuessWord,
            IHangmanRepositoryAsync<GameRound> repositoryGameRound,
            IHangmanRepositoryAsync<Player> repositoryPlayer,
            ILogger<GameRoomServiceAsync> logger,
            IHangmanGame gameLogic,
            IMapper mapper
            )
        {
            _repositoryGameRoomPlayer = repositoryGameRoomPlayer;
            _repositoryGuessLetter = repositoryGuessLetter;
            _repositoryGameRound = repositoryGameRound;
            _repositoryGuessWord = repositoryGuessWord;
            _repositoryPlayer = repositoryPlayer;
            _repository = repository;
            _gameLogic = gameLogic;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<GameRoom?> GetById(Guid id)
        {
            var gameRoom = await _repository.GetById(id);
            var includedFieldsOnSerialization = new[] { "GameRoomPlayers", "GuessWords" };

            await _repository.GetById(id, includedFieldsOnSerialization);
            return gameRoom;
        }

        public async Task<IEnumerable<GameRoom>> GetAll()
        {
            var includedFieldsOnSerialization = new[] { "GameRoomPlayers", "GuessWords" };
            var gameRooms = await _repository.All(includedFieldsOnSerialization);

            return gameRooms;
        }

        public async Task<IEnumerable<GuessWord>> GetAllGuessedWords(Guid gameRoomId)
        {
            var guessWords = await _repositoryGuessWord.Filter(word => word.GameRoom.Id == gameRoomId);

            return guessWords;
        }

        public async Task<GuessWord?> GetGuessedWord(Guid guessWordId)
        {
            var includedFieldsOnSerialization = new[] { "GameRoom", "Round", "GuessLetters" };
            var guessWord = await _repositoryGuessWord.GetById(guessWordId, includedFieldsOnSerialization);

            return guessWord;
        }

        public async Task<GameRoom> Create(GameRoomDTO gameRoomDTO)
        {
            var newGameRoom = _mapper.Map<GameRoomDTO, GameRoom>(gameRoomDTO);
            await _repository.Save(newGameRoom);

            return newGameRoom;
        }

        public async Task<PlayerInRoomDTO> JoinRoom(JoinRoomDTO joinRoomDTO)
        {
            var gameRoom = await _repository.GetById(joinRoomDTO.GameRoomId);
            var player = await _repositoryPlayer.GetById(joinRoomDTO.PlayerId);
            var previousGameRoomPlayer = await _repositoryGameRoomPlayer.Get(
                grp => (grp.PlayerId == player!.Id) && (grp.GameRoomId == joinRoomDTO.GameRoomId));

            if (previousGameRoomPlayer != null)
            {
                _logger.LogInformation("Player had previously join this room...");
                previousGameRoomPlayer.IsInRoom = true;
                await _repositoryGameRoomPlayer.Update(previousGameRoomPlayer);

                var alreadyJoinedRoomDTO = _mapper.Map<GameRoomPlayer, PlayerInRoomDTO>(previousGameRoomPlayer);
                return alreadyJoinedRoomDTO;
            }

            _logger.LogInformation("First time player is joining this room...");
            var gameRoomPlayer = new GameRoomPlayer()
            {
                GameRoom = gameRoom!,
                Player = player!,
                IsHost = joinRoomDTO.IsHost,
                IsBanned = false,
                IsInRoom = true,
            };
            await _repositoryGameRoomPlayer.Save(gameRoomPlayer);

            var playerInRoomDTO = _mapper.Map<GameRoomPlayer, PlayerInRoomDTO>(gameRoomPlayer);
            return playerInRoomDTO;
        }

        public async Task<PlayerInRoomDTO> LeaveRoom(LeaveRoomDTO leaveRoomDTO)
        {
            _logger.LogInformation("Player to leaving room: {@leaveRoomDTO}", leaveRoomDTO);
            var gameRoomPlayerData = await GetPlayerRoomData(leaveRoomDTO.GameRoomId, leaveRoomDTO.PlayerId);
            gameRoomPlayerData!.IsInRoom = false;  // previously validated, never null

            await _repositoryGameRoomPlayer.Update(gameRoomPlayerData);

            var playerInRoomDTO = _mapper.Map<GameRoomPlayer, PlayerInRoomDTO>(gameRoomPlayerData);
            return playerInRoomDTO;
        }

        public async Task<GameRoomPlayer?> GetPlayerRoomData(Guid gameRoomId, Guid playerId)
        {
            var gameRoomData =
                await _repositoryGameRoomPlayer.Get(grp => grp.GameRoomId == gameRoomId && grp.PlayerId == playerId);

            return gameRoomData;
        }

        public async Task<GuessWordResponseDTO> CreateGuessWord(GuessWordDTO guessWordDTO)
        {
            var gameRoom = await _repository.GetById(guessWordDTO.GameRoomId);  // previously validated
            var newGuessWord = new GuessWord()
            {
                GameRoom = gameRoom!,
                GameRoomId = gameRoom!.Id,
                Word = guessWordDTO.GuessWord
            };

            var gameRound = new GameRound
            {
                GuessWord = newGuessWord,
                GuessWordId = newGuessWord.Id
            };

            newGuessWord.Round = gameRound; // 1-to-1 relationship must be also created!
            await _repositoryGuessWord.Save(newGuessWord);

            return new GuessWordResponseDTO { Id = newGuessWord.Id, GuessWord = newGuessWord.Word };
        }

        public async Task<GuessLetter> CreateGuessLetter(GuessWord guessWord, string guessLetter)
        {
            var newGuessLetter = new GuessLetter()
            {
                GuessWord = guessWord,
                GuessWordId = guessWord.Id,
                Letter = guessLetter
            };

            await _repositoryGuessLetter.Save(newGuessLetter);
            return newGuessLetter;
        }

        public async Task<GameStateDTO> UpdateGameRoundState(NewGuessLetterDTO newGuessLetterDTO)
        {
            var guessWord = await GetGuessedWord(newGuessLetterDTO.GuessWordId);
            var newGuessLetterString = newGuessLetterDTO.GuessLetter;

            _logger.LogInformation("Persisting new player's move...");
            await CreateGuessLetter(guessWord!, newGuessLetterString);  // previously validated guess word

            var gameRound = guessWord!.Round;
            var allGuessLetters =
                guessWord.GuessLetters.Select(letter => letter.Letter); // with new guess letter already
            var updatedGameState = new GameStateDTO()
            {
                GuessWord = null,
                IsOver = false,
                PlayerHealth = guessWord.Round.Health,
                GuessWordSoFar = _gameLogic.GetGuessWordSoFar(allGuessLetters, guessWord.Word),
                GuessedLetters = allGuessLetters,
            };

            if (_gameLogic.IsGuessedLetterInGuessWord(newGuessLetterString, guessWord.Word))
            {
                _logger.LogInformation("Player guessed a right letter. Checking if turn is over...");
                if (_gameLogic.HasPlayerHasDiscoveredGuessWord(allGuessLetters, guessWord.Word))
                {
                    _logger.LogInformation("Guess word {:l} has been found! Turn is over...", guessWord.Word);
                    gameRound = _gameLogic.FinishGameRound(gameRound);
                    await _repositoryGameRound.Update(gameRound);

                    _logger.LogInformation("Setting final game state for return...");
                    updatedGameState.IsOver = true;
                    updatedGameState.GuessWord = guessWord.Word;
                }
            }
            else
            {
                _logger.LogInformation("Player has guessed a wrong letter. Punishing him with a health hit...");
                gameRound = _gameLogic.ReducePlayerHealth(gameRound);

                if (_gameLogic.HasPlayerBeenHung(gameRound))
                {
                    _logger.LogInformation("Player has been hung and is dead! Turn is over...");
                    gameRound = _gameLogic.FinishGameRound(gameRound);

                    _logger.LogInformation("Setting final game state for return...");
                    updatedGameState.IsOver = true;
                    updatedGameState.GuessWord = guessWord.Word;
                }

                await _repositoryGameRound.Update(gameRound);
            }

            updatedGameState.PlayerHealth = gameRound.Health;

            _logger.LogInformation("Returning updated game state data...");
            return updatedGameState;
        }

        public IEnumerable<string> GetGuessWordStateSoFar(GuessWord guessWord)
        {
            var allGuessedLetters = guessWord.GuessLetters.Select(letter => letter.Letter);
            return _gameLogic.GetGuessWordSoFar(allGuessedLetters, guessWord.Word);
        }
    }
}