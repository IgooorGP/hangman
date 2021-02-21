using AutoMapper;
using Hangman.Core.Models;

namespace Hangman.Core.DTOs.Mappings
{
    public class DTO2Domain : Profile
    {
        public DTO2Domain()
        {
            CreateMap<GameRoomDTO, GameRoom>();
            CreateMap<CreateGameRoomDTO, GameRoom>();
        }
    }
}