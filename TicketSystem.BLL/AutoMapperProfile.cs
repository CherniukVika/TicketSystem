using AutoMapper;
using TicketSystem.BLL.Dto;
using TicketSystem.DAL.Models;

namespace TicketSystem.BLL.Mapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Author, AuthorDto>();
            CreateMap<Genre, GenreDto>();
            CreateMap<Performance, PerformanceDto>()
                .ForMember(dest => dest.Genres, opt => opt.MapFrom(src => src.Genres))
                .ForMember(dest => dest.Tickets, opt => opt.MapFrom(src => src.Tickets))
                .ForMember(dest => dest.Schedules, opt => opt.MapFrom(src => src.Schedules));
            CreateMap<PerformanceSchedule, PerformanceScheduleDto>()
                .ForMember(dest => dest.Seats, opt => opt.MapFrom(src => src.Seats));
            CreateMap<Seat, SeatDto>()
                .ForMember(dest => dest.Ticket, opt => opt.MapFrom(src => src.Ticket));
            CreateMap<Ticket, TicketDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src =>
                    src.Status == TicketSystem.DAL.TicketStatus.Available ? TicketSystem.BLL.Dto.TicketStatus.Available :
                    src.Status == TicketSystem.DAL.TicketStatus.Sold ? TicketSystem.BLL.Dto.TicketStatus.Sold :
                    TicketSystem.BLL.Dto.TicketStatus.Returned));
        }
    }
}