using System;
using System.ComponentModel.DataAnnotations;

namespace Hangman.Core.Models
{
    /// <summary>
    /// Models a game round that is always related to a single GuessWord. The round ends when a player
    /// wins or loses by not guessing the GuessWord of the round.
    /// </summary>
    public class GameRound : BaseEntity
    {
        [Range(0, 6)]
        public int Health { get; set; } = 6;

        public bool IsOver { get; set; } = false;

        // 1-to-1 with GuessWord
        public GuessWord GuessWord { get; set; } = null!;

        public Guid GuessWordId { get; set; }
    }
}