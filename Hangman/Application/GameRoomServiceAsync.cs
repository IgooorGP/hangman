using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Hangman.Models;
using Hangman.Repository.Interfaces;
using Microsoft.Extensions.Logging;

namespace Hangman.Application
{
    public class NewGuessLetterData
    {
        [Required] public string GuessLetter { get; set; } = default!; // null-forgiving as this property is required
        [Required] public string PlayerName { get; set; } = default!; // null-forgiving as this property is required
    }
    
    public class NewGuessWordData
    {
        [Required] public string GuessWord { get; set; } = default!; // null-forgiving as this property is required
        [Required] public string PlayerName { get; set; } = default!; // null-forgiving as this property is required
    }

    public class JoinRoomData
    {
        [Required] public string PlayerName { get; set; } = default!; // null-forgiving as this property is required
    }

    public class NewGameRoomData
    {
        [Required] public string Name { get; set; } = default!;
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
        private readonly IHangmanRepositoryAsync<GameRoom> _repository;
        private readonly ILogger<GameRoomServiceAsync> _logger;

        public GameRoomServiceAsync(IHangmanRepositoryAsync<GameRoom> repository,
            IHangmanRepositoryAsync<GameRoomPlayer> repositoryGameRoomPlayer,
            IHangmanRepositoryAsync<GuessLetter> repositoryGuessLetter,
            IHangmanRepositoryAsync<GuessWord> repositoryGuessWord,
            IHangmanRepositoryAsync<GameRound> repositoryGameRound,
            ILogger<GameRoomServiceAsync> logger)
        {
            _repositoryGameRoomPlayer = repositoryGameRoomPlayer;
            _repositoryGuessLetter = repositoryGuessLetter;
            _repositoryGameRound = repositoryGameRound;
            _repositoryGuessWord = repositoryGuessWord;
            _repository = repository;
            _logger = logger;
        }

        public async Task<GameRoom?> GetById(Guid id)
        {
            var gameRoom = await _repository.GetById(id);
            var gameRoomPlayers = await _repositoryGameRoomPlayer.Filter(grp => grp.GameRoom == gameRoom);

            if (gameRoom != null) gameRoom.GameRoomPlayers = gameRoomPlayers.ToList();
            return gameRoom;
        }

        public async Task<IEnumerable<GameRoom>> GetAll()
        {
            var includedFieldsOnSerialization = new[] {"GameRoomPlayers", "GuessWords"};
            var gameRooms = await _repository.All(includedFieldsOnSerialization);

            return gameRooms;
        }

        public async Task<IEnumerable<GuessWord>> GetAllGuessedWords(Guid gameRoomId)
        {
            var guessWords = await _repositoryGuessWord.Filter(word => word.GameRoom.Id == gameRoomId);

            return guessWords;
        }

        public async Task<IEnumerable<GameRoom>> GetAllGuessWords()
        {
            var includedFieldsOnSerialization = new[] {"GameRoomPlayers", "GuessWords"};
            var gameRooms = await _repository.All(includedFieldsOnSerialization);

            return gameRooms;
        }

        public async Task<GameRoom> Create(NewGameRoomData newGameRoomData)
        {
            var newGameRoom = new GameRoom {Name = newGameRoomData.Name};
            await _repository.Save(newGameRoom);

            return newGameRoom;
        }

        // join room (without joining, can't make moves!)
        public async Task<GameRoomPlayer> JoinRoom(GameRoom gameRoom, Player player, bool isHost = false)
        {
            var previousGameRoomPlayer = await _repositoryGameRoomPlayer.Get(
                grp => (grp.PlayerId == player.Id) && (grp.GameRoomId == gameRoom.Id));

            if (previousGameRoomPlayer != null)
            {
                _logger.LogInformation("Player had previously join this room...");
                previousGameRoomPlayer.IsInRoom = true;
                await _repositoryGameRoomPlayer.Update(previousGameRoomPlayer);

                return previousGameRoomPlayer;
            }

            _logger.LogInformation("First time player is joining this room...");
            var gameRoomPlayer = new GameRoomPlayer()
            {
                GameRoom = gameRoom,
                Player = player,
                IsHost = isHost,
                IsBanned = false,
                IsInRoom = true,
            };

            await _repositoryGameRoomPlayer.Save(gameRoomPlayer);
            return gameRoomPlayer;
        }

        public async Task<GameRoomPlayer> LeaveRoom(GameRoomPlayer gameRoomPlayer)
        {
            _logger.LogInformation("Player {} is leaving room {}", gameRoomPlayer.PlayerId, gameRoomPlayer.GameRoomId);
            gameRoomPlayer.IsInRoom = false;

            await _repositoryGameRoomPlayer.Update(gameRoomPlayer);
            return gameRoomPlayer;
        }

        public async Task<GameRoomPlayer?> GetPlayerRoomData(GameRoom gameRoom, Player player)
        {
            var gameRoomData =
                await _repositoryGameRoomPlayer.Get(grp => grp.GameRoomId == gameRoom.Id && grp.PlayerId == player.Id);

            return gameRoomData;
        }

        public async Task<GuessWord> CreateGuessWord(GameRoom gameRoom, string guessWord)
        {
            var newGuessWord = new GuessWord()
            {
                GameRoom = gameRoom,
                GameRoomId = gameRoom.Id,
                Word = guessWord
            };

            await _repositoryGuessWord.Save(newGuessWord);
            return newGuessWord;
        }

        public async Task<GuessLetter> CreateGuessLetter(GameRound gameRound, string guessLetter)
        {
            var newGuessLetter = new GuessLetter()
            {
                GuessWord = gameRound.GuessWord,
                GuessWordId = gameRound.GuessWordId,
                Letter = guessLetter
            };

            await _repositoryGuessLetter.Save(newGuessLetter);
            return newGuessLetter;
        }
        
        public async Task<GuessLetter> UpdateGuessWordRoundState(GuessLetter guessLetter)
        {
            // 1.Update the game state: create new guess letter, check if word was found,
            //   check if letter applies or player takes a hit, check if player is alive
            // var gameRoom = guessWord.GameRoom;
            var guessWord = guessLetter.GuessWord;
            var gameRound = guessWord.Round;
            var previouslyGuessLetters = guessWord.GuessLetters;
        }
    }
}