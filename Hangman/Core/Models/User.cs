using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Hangman.Core.Models
{
    /// <summary>
    /// Models a player that can join GameRooms and create GuessWords (when hosting a room)
    /// and creating GuessLetters when playing.
    /// </summary>
    public class User : BaseEntity
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        // fk to join table for many-to-many relationships to access GameRooms
        // ignored on jsons: past of a user may bring too many results
        [JsonIgnore]
        public ICollection<GameRoomUser> GameRoomUsers { get; set; } = null!;
    }
}