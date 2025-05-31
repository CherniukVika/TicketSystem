using AutoMapper;
using TicketSystem.BLL.Dto;
using TicketSystem.PL.Models;

namespace TicketSystem.PL.Mappings
{
    public class PresentationMappingProfile : Profile
    {
        public PresentationMappingProfile()
        {
            CreateMap<PerformanceDto, PerformancePLDto>()
                .ForMember(dest => dest.AuthorName, opt => opt.MapFrom(src => src.Author.Name))
                .ForMember(dest => dest.Genres, opt => opt.MapFrom(src => src.Genres.Select(g => g.Name).ToList()));
            CreateMap<PerformanceScheduleDto, PerformanceSchedulePLDto>()
                .ForMember(dest => dest.AvailableSeats, opt => opt.Ignore());
            CreateMap<TicketDto, TicketPLDto>()
                .ForMember(dest => dest.SeatLocation, opt => opt.Ignore()) 
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        }
    }
}