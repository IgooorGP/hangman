using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Hangman.Core.Models
{
    /// <summary>
    /// Models a GuessLetter that is a player's attempt to discover the GuessWord of the GameRound.
    /// </summary>
    public class GuessLetter : BaseEntity
    {
        [Required]
        public GuessWord GuessWord { get; set; } = null!;

        [Required]
        public Guid GuessWordId { get; set; }

        [Required]
        [MaxLength(1)]
        public string Letter { get; set; } = null!;
    }
}