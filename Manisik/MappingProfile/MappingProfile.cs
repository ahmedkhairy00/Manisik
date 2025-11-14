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

            // map rooms
            CreateMap<HotelRoom, RoomDto>().ReverseMap();

            // ================================
            // Transport ↔ TransportDto
            // ================================
            CreateMap<GlobalTransport, TransportDto>().ReverseMap();

            // ================================
            // UmrahBooking ↔ UmrahBookingDto
            // يشمل تحويل قائمة BookingHotels
            // ================================
            CreateMap<Booking, UmrahBookingDto>()
                .ForMember(dest => dest.BookingHotels,
                           opt => opt.MapFrom(src => src.BookingHotels))
                .ReverseMap();

            // ================================
            // UmrahBookingHotel ↔ UmrahBookingHotelDto
            // ================================
            CreateMap<BookingHotel, UmrahBookingHotelDto>().ReverseMap();
        }
    }
}
