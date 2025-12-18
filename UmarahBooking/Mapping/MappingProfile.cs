using AutoMapper;
using UmarahBooking.Core.Enums;
using UmarahBooking.Core.Models;
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
            .ForMember(dest => dest.Rooms, opt => opt.MapFrom(src => src.Rooms))
            .ForMember(dest => dest.PricePerNight, opt => opt.MapFrom(src => src.Rooms != null && src.Rooms.Any() ? src.Rooms.Min(r => r.PricePerNight) : 0))
            .ForMember(dest => dest.AvailableRooms, opt => opt.MapFrom(src => src.Rooms != null ? src.Rooms.Sum(r => r.AvailableRooms) : 0));

            CreateMap<HotelDto, Hotel>()
            .ForMember(dest => dest.HotelId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.HotelCity, opt => opt.MapFrom(src => ParseHotelCity(src.City)))
            .ForMember(dest => dest.Rooms, opt => opt.Ignore());


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
                .ForMember(dest => dest.RoomType, opt => opt.MapFrom(src => ParseRoomType(src.RoomType)))
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
                .ForMember(dest => dest.NumberOfRooms, opt => opt.Condition((src, dest, srcMember) => srcMember > 0))
                .ForMember(dest => dest.BookingId, opt => opt.Ignore()); // set manually when creating booking


            // ------------------------------
            // BOOKING MODEL <-> BOOKING DTO
            // ------------------------------
            CreateMap<Booking, BookingDto>()
                // map primary key so frontend receives `Id`
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.BookingId))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.TripType.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.BookingStatus.ToString()))
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
                // ensure reverse mapping sets BookingId from Id
                .ForMember(dest => dest.BookingId, opt => opt.MapFrom(src => src.Id))
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
            // INTERNATIONAL TRANSPORT MODEL <-> DTO (Booking mapping)
            // ------------------------------
            CreateMap<BookingInternationalTransport, TransportBookingDto>()
                .ForMember(dest => dest.TransportId, opt => opt.MapFrom(src => src.InternationalTransportId))
                .ForMember(dest => dest.CarrierName, opt => opt.MapFrom(src => src.InternationalTransport.CarrierName))
                .ReverseMap()
                .ForMember(dest => dest.InternationalTransport, opt => opt.Ignore())
                .ForMember(dest => dest.InternationalTransportId, opt => opt.MapFrom(src => src.TransportId));

            // Map InternationalTransportBookingDto -> BookingInternationalTransport for creating bookings
            CreateMap<InternationalTransportBookingDto, BookingInternationalTransport>()
                .ForMember(dest => dest.InternationalTransportId, opt => opt.MapFrom(src => src.TransportId.HasValue ? src.TransportId.Value : 0))
                .ForMember(dest => dest.NumberOfSeats, opt => opt.MapFrom(src => src.NumberOfSeats))
                .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.TotalPrice.HasValue ? src.TotalPrice.Value : (src.NumberOfSeats * (src.PricePerSeat ?? 0m))))
                .ForMember(dest => dest.BookingId, opt => opt.Ignore())
                .ForMember(dest => dest.Booking, opt => opt.Ignore())
                .ForMember(dest => dest.InternationalTransport, opt => opt.Ignore());

            // ------------------------------
            // GROUND TRANSPORT MODEL <-> DTO (Booking mapping)
            // ------------------------------
            CreateMap<BookingGroundTransport, GroundTransportBookingDto>()
                .ForMember(dest => dest.GroundTransportId, opt => opt.MapFrom(src => src.GroundTransportId))
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.GroundTransport.ServiceName))
                .ReverseMap()
                .ForMember(dest => dest.GroundTransport, opt => opt.Ignore())
                .ForMember(dest => dest.GroundTransportId, opt => opt.MapFrom(src => src.GroundTransportId));

            // Map GroundTransportBookingDto -> BookingGroundTransport for creating bookings
            CreateMap<GroundTransportBookingDto, BookingGroundTransport>()
                .ForMember(dest => dest.GroundTransportId, opt => opt.MapFrom(src => src.GroundTransportId))
                .ForMember(dest => dest.NumberOfPassengers, opt => opt.MapFrom(src => src.NumberOfPassengers))
                .ForMember(dest => dest.PickupLocation, opt => opt.MapFrom(src => src.PickupLocation))
                .ForMember(dest => dest.DropoffLocation, opt => opt.MapFrom(src => src.DropoffLocation))
                .ForMember(dest => dest.ServiceDate, opt => opt.MapFrom(src => src.ServiceDate))
                .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.TotalPrice.HasValue ? src.TotalPrice.Value : (src.NumberOfPassengers * (src.PricePerPerson ?? 0m))))
                .ForMember(dest => dest.BookingId, opt => opt.Ignore())
                .ForMember(dest => dest.Booking, opt => opt.Ignore())
                .ForMember(dest => dest.GroundTransport, opt => opt.Ignore());

            // ------------------------------
            // PAYMENT MODEL <-> PAYMENT DTO
            // ------------------------------
            CreateMap<Payment, PaymentDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.PaymentId))
                .ReverseMap()
                .ForMember(dest => dest.PaymentId, opt => opt.Ignore())
                .ForMember(dest => dest.Booking, opt => opt.Ignore());

            // ------------------------------
            // APPLICATION USER <-> USER DTO
            // ------------------------------
            CreateMap<ApplicationUser, UserDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt))
                // We'll set FirstName/LastName in AfterMap to avoid expression-tree restrictions
                .AfterMap((src, dest) =>
                {
                    if (!string.IsNullOrWhiteSpace(src.FullName))
                    {
                        var parts = src.FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        dest.FirstName = parts.Length > 0 ? parts[0] : string.Empty;
                        dest.LastName = parts.Length > 1 ? string.Join(' ', parts.Skip(1)) : string.Empty;
                    }
                    else
                    {
                        dest.FirstName = string.Empty;
                        dest.LastName = string.Empty;
                    }
                });

            // ------------------------------
            // BOOKING SUMMARY (for UserWithBookingsDto)
            // ------------------------------
            CreateMap<Booking, BookingSummaryDto>()
                .ForMember(dest => dest.BookingId, opt => opt.MapFrom(src => src.BookingId))
                .ForMember(dest => dest.BookingNumber, opt => opt.MapFrom(src => src.BookingNumber ?? string.Empty))
                .ForMember(dest => dest.BookingType, opt => opt.MapFrom(src => src.TripType.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.BookingStatus.ToString()))
                .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.TotalPrice ?? 0m))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt ?? DateTime.UtcNow))
                .ForMember(dest => dest.HotelsCount, opt => opt.MapFrom(src => src.Hotels != null ? src.Hotels.Count : 0))
                .ForMember(dest => dest.TravelersCount, opt => opt.MapFrom(src => src.Travelers != null ? src.Travelers.Count : 0));

            // ------------------------------
            // APPLICATION USER -> USER WITH BOOKINGS
            // ------------------------------
            CreateMap<ApplicationUser, UserWithBookingsDto>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
                .ForMember(dest => dest.Country, opt => opt.MapFrom(src => src.Country))
                .ForMember(dest => dest.Bookings, opt => opt.MapFrom(src => src.Bookings));

            // ------------------------------
            // GROUND TRANSPORT ENTITY <-> DTO
            // ------------------------------
            CreateMap<GroundTransport, GroundTransportDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.GroundTransportId))
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.ServiceName))
                .ForMember(dest => dest.ServiceNameAr, opt => opt.Ignore())
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.InternalTransportType.ToString()))
                .ForMember(dest => dest.PricePerPerson, opt => opt.MapFrom(src => src.PricePerPerson))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.DescriptionAr, opt => opt.Ignore())
                .ForMember(dest => dest.Capacity, opt => opt.MapFrom(src => src.Capacity))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(dest => dest.GroundTransportId, opt => opt.MapFrom(src => src.Id.HasValue ? src.Id.Value : 0))
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.ServiceName))
                .ForMember(dest => dest.InternalTransportType, opt => opt.MapFrom(src => ParseInternalTransportType(src.Type)))
                .ForMember(dest => dest.PricePerPerson, opt => opt.MapFrom(src => src.PricePerPerson))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Capacity, opt => opt.MapFrom(src => src.Capacity))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));

            // ------------------------------
            // INTERNATIONAL TRANSPORT ENTITY <-> DTO
            // ------------------------------
            CreateMap<InternationalTransport, InternationalTransportDto>()
           .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.InternationalTransportId))
           .ForMember(dest => dest.TransportType, opt => opt.MapFrom(src => src.TransportType.ToString()))
           .ForMember(dest => dest.CarrierName, opt => opt.MapFrom(src => src.CarrierName))
           .ForMember(dest => dest.DepartureAirport, opt => opt.MapFrom(src => src.DepartureAirport.ToString()))
           .ForMember(dest => dest.DepartureAirportCode, opt => opt.Ignore())
           .ForMember(dest => dest.ArrivalAirport, opt => opt.MapFrom(src => src.ArrivalAirport.ToString()))
           .ForMember(dest => dest.ArrivalAirportCode, opt => opt.Ignore())
           .ForMember(dest => dest.DepartureDate, opt => opt.MapFrom(src => src.DepartureDate))
           .ForMember(dest => dest.ArrivalDate, opt => opt.MapFrom(src => src.ArrivalDate))
           .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price))
           .ForMember(dest => dest.TotalSeats, opt => opt.MapFrom(src => src.TotalSeats))
           .ForMember(dest => dest.AvailableSeats, opt => opt.MapFrom(src => src.AvailableSeats))
           .ForMember(dest => dest.FlightNumber, opt => opt.MapFrom(src => src.FlightNumber))
           .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
           .ForMember(dest => dest.FlightClass, opt => opt.MapFrom(src => src.FlightClass.ToString()))
           .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.Duration))
           .ForMember(dest => dest.Stops, opt => opt.MapFrom(src => src.Stops))
           .ReverseMap()
           .ForMember(dest => dest.InternationalTransportId, opt => opt.MapFrom(src => src.Id))
           .ForMember(dest => dest.TransportType, opt => opt.MapFrom(src => ParseInternationalTransportType(src.TransportType)))
           .ForMember(dest => dest.DepartureAirport, opt => opt.MapFrom(src => ParseDepartureAirport(src.DepartureAirport)))
           .ForMember(dest => dest.ArrivalAirport, opt => opt.MapFrom(src => ParseArrivalAirport(src.ArrivalAirport)))
           .ForMember(dest => dest.FlightClass, opt => opt.MapFrom(src => ParseFlightClass(src.FlightClass)))
           .ForMember(dest => dest.BookingInternationalTransport, opt => opt.Ignore());

        }

        // Helper methods used in MapFrom expressions — allowed because MapFrom references method call only
        private static InternationalTransportType ParseInternationalTransportType(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return InternationalTransportType.Plane;
            return Enum.TryParse<InternationalTransportType>(value, true, out var t) ? t : InternationalTransportType.Plane;
        }

        private static DepartureAirport ParseDepartureAirport(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return DepartureAirport.Cairo;
            return Enum.TryParse<DepartureAirport>(value, true, out var d) ? d : DepartureAirport.Cairo;
        }

        private static ArrivalAirport ParseArrivalAirport(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return ArrivalAirport.Jeddah;
            return Enum.TryParse<ArrivalAirport>(value, true, out var a) ? a : ArrivalAirport.Jeddah;
        }

        private static flightDegree ParseFlightClass(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return flightDegree.Economy;
            return Enum.TryParse<flightDegree>(value, true, out var result) ? result : flightDegree.Economy;
        }

        private static InternalTransportType ParseInternalTransportType(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return InternalTransportType.PrivateCar;
            return Enum.TryParse<InternalTransportType>(value, true, out var result) ? result : InternalTransportType.PrivateCar;
        }

        private static HotelCity ParseHotelCity(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return HotelCity.Makkah;
            return Enum.TryParse<HotelCity>(value, true, out var result) ? result : HotelCity.Makkah;
        }

        private static RoomType ParseRoomType(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return RoomType.Single;
            return Enum.TryParse<RoomType>(value, true, out var result) ? result : RoomType.Single;
        }

    }
}

