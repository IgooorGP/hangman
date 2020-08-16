using System;
using System.ComponentModel.DataAnnotations;

namespace Hangman.DTOs
{
    public class NewGuessLetterRequestDTO
    {
        public Guid PlayerId { get; set; }

        public string GuessLetter { get; set; } = null!;
    }

    public class NewGuessLetterDTO
    {
        public Guid GameRoomId { get; set; }

        public Guid GuessWordId { get; set; }

        public Guid PlayerId { get; set; }

        public string GuessLetter { get; set; } = null!;
    }
}