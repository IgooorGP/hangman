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
        public string Value { get; set; } = null!;

        // many-to-one fk to GameRoom
        public Guid GameRoomId { get; set; }
        public GameRoom GameRoom { get; set; } = null!;

        // one-to-one to Round (unique)
        public GameRound Round { get; set; } = null!;

        // one-to-many with GuessLetter
        public ICollection<GuessLetter> GuessLetters { get; set; } = null!;
    }
}