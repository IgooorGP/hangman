using System;
using System.Collections.Generic;

namespace Hangman.Core.Models
{
    /// <summary>
    /// Models a game room in which a guess words are created and are joined by
    /// many players.
    /// </summary>
    public class GameRoom : BaseEntity
    {
        public string Name { get; set; } = null!;

        // fk to join table for many-to-many relationships to access Users
        public ICollection<GameRoomUser> GameRoomUsers { get; set; } = null!;

        // one-to-many
        public ICollection<GuessWord> GuessWords { get; set; } = null!;
    }

    /// <summary>
    /// Intertable for wiring-up the many-to-many relationship between GameRooms and Players.
    /// </summary>
    public class GameRoomUser : BaseEntity
    {
        // one-to-many with GameRoom
        public Guid GameRoomId { get; set; }
        public GameRoom GameRoom { get; set; } = null!;

        // one-to-many with User
        public User User { get; set; } = null!;
        public Guid UserId { get; set; }

        // extra interjoin table properties
        public bool IsHost { get; set; }
        public bool IsInRoom { get; set; }
    }
}