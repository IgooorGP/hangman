using System;

namespace Hangman.Core.Models
{
    /// <summary>
    /// Models a GuessLetter that is a player's attempt to discover the GuessWord of the GameRound.
    /// </summary>
    public class GuessLetter : BaseEntity
    {
        public string Value { get; set; } = string.Empty;

        // many-to-one fk with GuessWord
        public Guid GuessWordId { get; set; }
        public GuessWord GuessWord { get; set; } = null!;
    }
}