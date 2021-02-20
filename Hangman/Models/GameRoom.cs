using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace Hangman.Models
{
    /// <summary>
    /// Models a game room in which a guess words are created and are joined by
    /// many players.
    /// </summary>
    public class GameRoom : BaseEntity
    {
        [Required]
        [MaxLength(255)]
        public string Name { get; set; } = null!;

        // fk to join table for many-to-many relationships to access Users
        public ICollection<GameRoomPlayer> GameRoomPlayers { get; set; } = null!;

        // one-to-many
        public ICollection<GuessWord> GuessWords { get; set; } = null!;
    }

    /// <summary>
    /// Intertable for wiring-up the many-to-many relationship between GameRoom and Player.
    /// </summary>
    public class GameRoomPlayer : BaseEntity
    {
        [Required]
        public Guid GameRoomId { get; set; }

        [Required]
        public GameRoom GameRoom { get; set; } = null!;

        [Required]
        public Player Player { get; set; } = null!;

        [Required]
        public Guid PlayerId { get; set; }

        [Required]
        public bool IsHost { get; set; }

        [Required]
        public bool IsBanned { get; set; }

        [Required]
        public bool IsInRoom { get; set; }
    }
}