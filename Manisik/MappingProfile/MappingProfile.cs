using AutoMapper;
using Manisik.DTOs;
using Manisik.Models;

namespace Manisik.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ================================
            // Auth → AuthResponseDto
            // ================================
            CreateMap<Auth, AuthResponseDto>();

            // ================================
            // Hotel ↔ HotelDto
            // ================================
            CreateMap<Hotel, HotelDto>().ReverseMap();

            // ================================
            // Transport ↔ TransportDto
            // ================================
            CreateMap<Transport, TransportDto>().ReverseMap();

            // ================================
            // UmrahBooking ↔ UmrahBookingDto
            // يشمل تحويل قائمة BookingHotels
            // ================================
            CreateMap<UmrahBooking, UmrahBookingDto>()
                .ForMember(dest => dest.BookingHotels,
                           opt => opt.MapFrom(src => src.BookingHotels))
                .ReverseMap();

            // ================================
            // UmrahBookingHotel ↔ UmrahBookingHotelDto
            // ================================
            CreateMap<UmrahBookingHotel, UmrahBookingHotelDto>().ReverseMap();
        }
    }
}
