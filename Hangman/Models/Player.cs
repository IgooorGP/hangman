using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Hangman.Models
{
    /// <summary>
    /// Models a player that can join GameRooms and create GuessWords (when hosting a room)
    /// and creating GuessLetters when playing.
    /// </summary>
    public class Player : BaseEntity
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = null!;

        // fk to join table for many-to-many relationships to access GameRooms
        // ignored on jsons: past of a user may bring too many results
        [JsonIgnore]
        public ICollection<GameRoomPlayer> GameRoomPlayers { get; set; } = null!;
    }
}