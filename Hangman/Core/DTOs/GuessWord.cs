using System;

namespace Hangman.Core.DTOs
{
    public class GuessWordRequestDTO
    {
        public string GuessWord { get; set; } = null!;
        public Guid PlayerId { get; set; }
    }
    public class GuessWordDTO
    {
        public string GuessWord { get; set; } = null!;
        public Guid PlayerId { get; set; }
        public Guid GameRoomId { get; set; }
    }

    public class GuessWordInGuessRoomDTO
    {
        public Guid GameRoomId { get; set; }
        public Guid GuessWordId { get; set; }
    }

    public class GuessWordResponseDTO
    {
        public string GuessWord { get; set; } = null!;
        public Guid Id { get; set; }  // created guess word id
    }
}