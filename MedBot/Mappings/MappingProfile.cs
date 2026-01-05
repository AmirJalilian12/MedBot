using AutoMapper;
using MedBot.Dtos;
using MedBot.Entities;

namespace MedBot.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<NewsDto, News>().ReverseMap();
        }
    }
}
