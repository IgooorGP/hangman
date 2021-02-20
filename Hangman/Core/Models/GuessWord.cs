using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Hangman.Core.Models
{
    /// <summary>
    /// Models a guess word that was created in a GameRoom.
    /// </summary>
    public class GuessWord : BaseEntity
    {
        [Required]
        public GameRoom GameRoom { get; set; } = null!;

        [Required]
        public Guid GameRoomId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Word { get; set; } = null!;

        public GameRound Round { get; set; } = null!;

        public ICollection<GuessLetter> GuessLetters { get; set; } = null!;
    }
}