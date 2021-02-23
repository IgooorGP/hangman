using System.Linq;
using AutoMapper;
using Hangman.Core.Models;

namespace Hangman.Core.DTOs.Mappings
{
    public class Domain2DTO : Profile
    {
        public Domain2DTO()
        {
            CreateMap<GuessWord, GuessWordResponseDTO>();
            CreateMap<GameRoom, GameRoomResponseDTO>()
                .ForMember(dst => dst.Players,
                    opt => opt.MapFrom(src => src.GameRoomUsers.Select(grp => grp.User)))
                .ForMember(dst => dst.GuessWords,
                    opt => opt.MapFrom(src => src.GuessWords.Select(word => word)));
        }
    }
}