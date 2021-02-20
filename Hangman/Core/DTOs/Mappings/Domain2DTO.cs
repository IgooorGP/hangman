using AutoMapper;
using Hangman.Core.Models;

namespace Hangman.Core.DTOs.Mappings
{
    public class Domain2DTO : Profile
    {
        public Domain2DTO()
        {
            CreateMap<GameRoomPlayer, PlayerInRoomDTO>();
        }
    }
}