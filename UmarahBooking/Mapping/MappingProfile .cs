using AutoMapper;
using Manisik.Enums;
using Manisik.Models;
using UmarahBooking.Core.DTO;

namespace UmarahBooking.Core.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ------------------------------
            // HOTEL MODEL <-> HOTEL DTO
            // ------------------------------
            CreateMap<Hotel, HotelDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.HotelId))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.HotelCity.ToString()))
                .ForMember(dest => dest.Rooms, opt => opt.MapFrom(src => src.Rooms));


            CreateMap<HotelDto, Hotel>()
                .ForMember(dest => dest.HotelId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.HotelCity, opt => opt.MapFrom(src => src.City))
                .ForMember(dest => dest.Rooms, opt => opt.Ignore()); // handled separately

            // ------------------------------
            // HOTEL ROOM MODEL <-> ROOM DTO
            // ------------------------------
            CreateMap<HotelRoom, RoomDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.HotelRoomId))
                .ForMember(dest => dest.TotalRooms, opt => opt.MapFrom(src => src.AvailableRooms))
                .ForMember(dest => dest.HotelId, opt => opt.MapFrom(src => src.HotelId))
                .ForMember(dest => dest.RoomType, opt => opt.MapFrom(src => src.RoomType.ToString()));


            CreateMap<RoomDto, HotelRoom>()
                .ForMember(dest => dest.HotelRoomId, opt => opt.Ignore()) // DB generated
                .ForMember(dest => dest.AvailableRooms, opt => opt.MapFrom(src => src.TotalRooms));

            // ------------------------------
            // BOOKING HOTEL MODEL <-> HOTEL BOOKING DTO
            // ------------------------------
            CreateMap<BookingHotel, HotelBookingDto>()
                .ForMember(dest => dest.HotelName, opt => opt.MapFrom(src => src.Hotel.Name))
                .ForMember(dest => dest.RoomType, opt => opt.MapFrom(src => src.Room.RoomType))
                .ForMember(dest => dest.NumberOfNights, opt => opt.MapFrom(src =>
                    (int)((src.CheckOutDate - src.CheckInDate).TotalDays)))
                .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.TotalPrice))
                .ForMember(dest => dest.City, opt => opt.MapFrom(src => src.City.ToString()))
                .ReverseMap()
                .ForMember(dest => dest.Hotel, opt => opt.Ignore())
                .ForMember(dest => dest.Room, opt => opt.Ignore())
                .ForMember(dest => dest.TotalPrice, opt => opt.Ignore()) // calculated in service
                .ForMember(dest => dest.NumberOfRooms, opt => opt.Condition(src => src.NumberOfRooms > 0))
                .ForMember(dest => dest.BookingId, opt => opt.Ignore()); // set manually when creating booking


            // ------------------------------
            // BOOKING MODEL <-> BOOKING DTO
            // ------------------------------
            CreateMap<Booking, BookingDto>()
                .ForMember(dest => dest.MakkahHotel,
                    opt => opt.MapFrom(src => src.Hotels.FirstOrDefault(bh => bh.City == HotelCity.Makkah)))
                .ForMember(dest => dest.MadinahHotel,
                    opt => opt.MapFrom(src => src.Hotels.FirstOrDefault(bh => bh.City == HotelCity.Madinah)))
                .ForMember(dest => dest.Travelers, opt => opt.MapFrom(src => src.Travelers))
                .ForMember(dest => dest.InternationalTransport,
                    opt => opt.MapFrom(src => src.BookingInternationalTransport.FirstOrDefault()))
                .ForMember(dest => dest.GroundTransport,
                    opt => opt.MapFrom(src => src.BookingGroundTransport.FirstOrDefault()))
                .ForMember(dest => dest.Payment, opt => opt.MapFrom(src => src.Payment))
                .ReverseMap()
                .ForMember(dest => dest.Hotels, opt => opt.Ignore())
                .ForMember(dest => dest.Travelers, opt => opt.Ignore())
                .ForMember(dest => dest.BookingInternationalTransport, opt => opt.Ignore())
                .ForMember(dest => dest.BookingGroundTransport, opt => opt.Ignore())
                .ForMember(dest => dest.Payment, opt => opt.Ignore());

            // ------------------------------
            // TRAVELER MODEL <-> TRAVELER DTO
            // ------------------------------
            CreateMap<Traveler, TravelerDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.TravelerId))
                .ReverseMap()
                .ForMember(dest => dest.TravelerId, opt => opt.Ignore());

            // ------------------------------
            // INTERNATIONAL TRANSPORT MODEL <-> DTO
            // ------------------------------
            CreateMap<BookingInternationalTransport, TransportBookingDto>()
                .ForMember(dest => dest.TransportId, opt => opt.MapFrom(src => src.InternationalTransportId))
                .ForMember(dest => dest.CarrierName, opt => opt.MapFrom(src => src.InternationalTransport.CarrierName))
                .ReverseMap()
                .ForMember(dest => dest.InternationalTransport, opt => opt.Ignore())
                .ForMember(dest => dest.InternationalTransportId, opt => opt.MapFrom(src => src.TransportId));

            // ------------------------------
            // GROUND TRANSPORT MODEL <-> DTO
            // ------------------------------
            CreateMap<BookingGroundTransport, GroundTransportBookingDto>()
                .ForMember(dest => dest.GroundTransportId, opt => opt.MapFrom(src => src.GroundTransportId))
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.GroundTransport.ServiceName))
                .ReverseMap()
                .ForMember(dest => dest.GroundTransport, opt => opt.Ignore())
                .ForMember(dest => dest.GroundTransportId, opt => opt.MapFrom(src => src.GroundTransportId));

            // ------------------------------
            // PAYMENT MODEL <-> PAYMENT DTO
            // ------------------------------
            CreateMap<Payment, PaymentDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.PaymentId))
                .ReverseMap()
                .ForMember(dest => dest.PaymentId, opt => opt.Ignore())
                .ForMember(dest => dest.Booking, opt => opt.Ignore());
        }
    }
}
