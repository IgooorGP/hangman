using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Hangman.Core.DTOs
{
    public class SearchGameRoomDTO
    {
        public int PageSize { get; set; } = 10;
        public int PageNumber { get; set; } = 1;
        public string? Name { get; set; }
    }

    public class CreateGameRoomDTO
    {
        public string Name { get; set; } = "";
    }

    public class GameRoomResponseDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
        public ICollection<UserResponseDTO>? Players { get; set; }
        public ICollection<GuessWordResponseDTO>? GuessWords { get; set; }
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
}