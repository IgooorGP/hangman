using System.Collections.Generic;
using Newtonsoft.Json;

namespace Hangman.Core.Models
{
    /// <summary>
    /// Models a player that can join GameRooms and create GuessWords (when hosting a room)
    /// and creating GuessLetters when playing.
    /// </summary>
    public class User : BaseEntity
    {
        // Security-related data
        public string Username { get; set; } = string.Empty;
        public string PasswordDigest { get; set; } = string.Empty;
        public string PasswordSalt { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        // User data
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;

        // fk to join table for many-to-many relationships to access GameRooms
        // ignored on jsons: past of a user may bring too many results
        [JsonIgnore]
        public ICollection<GameRoomUser> GameRoomUsers { get; set; } = null!;
    }
}