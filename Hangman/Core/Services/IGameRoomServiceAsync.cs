using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hangman.Core.DTOs;
using Hangman.Core.Models;

namespace Hangman.Core.Services
{
    public interface IGameRoomServiceAsync
    {
        public Task<GameRoom?> GetById(Guid id);
        public Task<IEnumerable<GameRoom>> GetAll();
        public Task<IEnumerable<GuessWord>> GetAllGuessedWords(Guid gameRoomId);
        public Task<GuessWord?> GetGuessedWord(Guid guessWordId);
        public Task<GameRoom> Create(GameRoomDTO gameRoomDTO);
        public Task<GuessWordResponseDTO> CreateGuessWord(GuessWordDTO guessWordDTO);
        public Task<PlayerInRoomDTO> JoinRoom(JoinRoomDTO joinRoomDTO);
        public Task<PlayerInRoomDTO> LeaveRoom(LeaveRoomDTO leaveRoomDTO);
        public Task<GameRoomPlayer?> GetPlayerRoomData(Guid gameRoomId, Guid playerId);
        public Task<GuessLetter> CreateGuessLetter(GuessWord guessWord, string guessLetter);
        public Task<GameStateDTO> UpdateGameRoundState(NewGuessLetterDTO newGuessLetterDTO);
        public IEnumerable<string> GetGuessWordStateSoFar(GuessWord guessWord);
    }
}