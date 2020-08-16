using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Hangman.Models;

namespace Hangman.DTOs
{
    public class GameRoomDTO
    {
        [Required]
        [StringLength(100, ErrorMessage = "Game room name can't exceed 100 characters.")]
        public string Name { get; set; } = default!;
    }

    public class PlayerDTO
    {
        public Guid PlayerId { get; set; }
    }

    public class JoinRoomDTO
    {
        [Required]
        public Guid GameRoomId { get; set; }

        [Required]
        public Guid PlayerId { get; set; }

        public bool IsHost { get; set; }
    }

    public class LeaveRoomDTO
    {
        [Required]
        public Guid GameRoomId { get; set; }

        [Required]
        public Guid PlayerId { get; set; }
    }

    public class PlayerInRoomDTO
    {
        public Guid playerId { get; set; }

        public Guid gameRoomId { get; set; }

        public bool isInRoom { get; set; }
    }
}