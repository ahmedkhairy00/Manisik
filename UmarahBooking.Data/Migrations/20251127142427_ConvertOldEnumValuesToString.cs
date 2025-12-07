using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UmarahBooking.Data.Migrations
{
    public partial class ConvertOldEnumValuesToString : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Payments
            migrationBuilder.Sql("UPDATE Payments SET PaymentMethod = 'Stripe' WHERE PaymentMethod = '0'");
            migrationBuilder.Sql("UPDATE Payments SET PaymentMethod = 'PayPal' WHERE PaymentMethod = '1'");
            migrationBuilder.Sql("UPDATE Payments SET Status = 'Pending' WHERE Status = '0'");
            migrationBuilder.Sql("UPDATE Payments SET Status = 'Confirmed' WHERE Status = '1'");
            migrationBuilder.Sql("UPDATE Payments SET Status = 'Cancelled' WHERE Status = '2'");
            migrationBuilder.Sql("UPDATE Payments SET Status = 'Refunded' WHERE Status = '3'");
            migrationBuilder.Sql("UPDATE Payments SET Status = 'Paid' WHERE Status = '4'");

            // Bookings
            migrationBuilder.Sql("UPDATE Bookings SET PaymentMethod = 'Stripe' WHERE PaymentMethod = '0'");
            migrationBuilder.Sql("UPDATE Bookings SET PaymentMethod = 'PayPal' WHERE PaymentMethod = '1'");
            migrationBuilder.Sql("UPDATE Bookings SET PaymentStatus = 'Pending' WHERE PaymentStatus = '0'");
            migrationBuilder.Sql("UPDATE Bookings SET PaymentStatus = 'Confirmed' WHERE PaymentStatus = '1'");
            migrationBuilder.Sql("UPDATE Bookings SET PaymentStatus = 'Cancelled' WHERE PaymentStatus = '2'");
            migrationBuilder.Sql("UPDATE Bookings SET PaymentStatus = 'Refunded' WHERE PaymentStatus = '3'");
            migrationBuilder.Sql("UPDATE Bookings SET PaymentStatus = 'Paid' WHERE PaymentStatus = '4'");
            migrationBuilder.Sql("UPDATE Bookings SET BookingStatus = 'Pending' WHERE BookingStatus = '0'");
            migrationBuilder.Sql("UPDATE Bookings SET BookingStatus = 'Confirmed' WHERE BookingStatus = '1'");
            migrationBuilder.Sql("UPDATE Bookings SET BookingStatus = 'Cancelled' WHERE BookingStatus = '2'");
            migrationBuilder.Sql("UPDATE Bookings SET BookingStatus = 'Refunded' WHERE BookingStatus = '3'");
            migrationBuilder.Sql("UPDATE Bookings SET TripType = 'Umrah' WHERE TripType = '0'");
            migrationBuilder.Sql("UPDATE Bookings SET TripType = 'Hajj' WHERE TripType = '1'");

            // Travelers
            migrationBuilder.Sql("UPDATE Travelers SET Gender = 'Male' WHERE Gender = '0'");
            migrationBuilder.Sql("UPDATE Travelers SET Gender = 'Female' WHERE Gender = '1'");

            // Hotels
            migrationBuilder.Sql("UPDATE Hotels SET HotelCity = 'Makkah' WHERE HotelCity = '0'");
            migrationBuilder.Sql("UPDATE Hotels SET HotelCity = 'Madinah' WHERE HotelCity = '1'");

            // BookingHotels
            migrationBuilder.Sql("UPDATE BookingHotels SET City = 'Makkah' WHERE City = '0'");
            migrationBuilder.Sql("UPDATE BookingHotels SET City = 'Madinah' WHERE City = '1'");

            // GroundTransports
            migrationBuilder.Sql("UPDATE GroundTransports SET InternalTransportType = 'PrivateCar' WHERE InternalTransportType = '0'");
            migrationBuilder.Sql("UPDATE GroundTransports SET InternalTransportType = 'SharedBus' WHERE InternalTransportType = '1'");
            migrationBuilder.Sql("UPDATE GroundTransports SET InternalTransportType = 'Taxi' WHERE InternalTransportType = '2'");

            // InternationalTransports
            migrationBuilder.Sql("UPDATE InternationalTransports SET TransportType = 'Plane' WHERE TransportType = '0'");
            migrationBuilder.Sql("UPDATE InternationalTransports SET TransportType = 'Ship' WHERE TransportType = '1'");
            migrationBuilder.Sql("UPDATE InternationalTransports SET DepartureAirport = 'CairoInternational' WHERE DepartureAirport = '0'");
            migrationBuilder.Sql("UPDATE InternationalTransports SET DepartureAirport = 'BorgElArabAlexandria' WHERE DepartureAirport = '1'");
            migrationBuilder.Sql("UPDATE InternationalTransports SET DepartureAirport = 'SharmElSheikhInternational' WHERE DepartureAirport = '2'");
            migrationBuilder.Sql("UPDATE InternationalTransports SET DepartureAirport = 'HurghadaInternational' WHERE DepartureAirport = '3'");
            migrationBuilder.Sql("UPDATE InternationalTransports SET DepartureAirport = 'AssiutInternational' WHERE DepartureAirport = '4'");
            migrationBuilder.Sql("UPDATE InternationalTransports SET DepartureAirport = 'SohagInternational' WHERE DepartureAirport = '5'");
            migrationBuilder.Sql("UPDATE InternationalTransports SET ArrivalAirport = 'Jeddah' WHERE ArrivalAirport = '0'");
            migrationBuilder.Sql("UPDATE InternationalTransports SET ArrivalAirport = 'Madinah' WHERE ArrivalAirport = '1'");
            migrationBuilder.Sql("UPDATE InternationalTransports SET ArrivalAirport = 'Taif' WHERE ArrivalAirport = '2'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // optional: leave empty if you don't need reverse
        }
    }
}
