    using Microsoft.EntityFrameworkCore.Migrations;

    #nullable disable

    namespace UmarahBooking.Data.Migrations
    {
        /// <inheritdoc />
        public partial class MakeEnumStringAppear : Migration
        {
            /// <inheritdoc />
            protected override void Up(MigrationBuilder migrationBuilder)
            {
                migrationBuilder.AlterColumn<string>(
                    name: "Gender",
                    table: "Travelers",
                    type: "varchar(50)",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "int");

                migrationBuilder.AlterColumn<string>(
                    name: "Status",
                    table: "Payments",
                    type: "varchar(50)",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "int");

                migrationBuilder.AlterColumn<string>(
                    name: "PaymentMethod",
                    table: "Payments",
                    type: "varchar(50)",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "int");

                migrationBuilder.AlterColumn<string>(
                    name: "TransportType",
                    table: "InternationalTransports",
                    type: "varchar(50)",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "int");

                migrationBuilder.AlterColumn<string>(
                    name: "DepartureAirport",
                    table: "InternationalTransports",
                    type: "varchar(50)",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "int");

                migrationBuilder.AlterColumn<string>(
                    name: "ArrivalAirport",
                    table: "InternationalTransports",
                    type: "varchar(50)",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "int");

                migrationBuilder.AlterColumn<string>(
                    name: "HotelCity",
                    table: "Hotels",
                    type: "varchar(50)",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "int");

                migrationBuilder.AlterColumn<string>(
                    name: "InternalTransportType",
                    table: "GroundTransports",
                    type: "varchar(50)",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "int");

                migrationBuilder.AlterColumn<string>(
                    name: "TripType",
                    table: "Bookings",
                    type: "varchar(50)",
                    nullable: false,
                    defaultValue: "",
                    oldClrType: typeof(int),
                    oldType: "int",
                    oldNullable: true);

                migrationBuilder.AlterColumn<string>(
                    name: "PaymentStatus",
                    table: "Bookings",
                    type: "varchar(50)",
                    nullable: true,
                    oldClrType: typeof(int),
                    oldType: "int",
                    oldNullable: true);

                migrationBuilder.AlterColumn<string>(
                    name: "PaymentMethod",
                    table: "Bookings",
                    type: "varchar(50)",
                    nullable: true,
                    oldClrType: typeof(int),
                    oldType: "int",
                    oldNullable: true);

                migrationBuilder.AlterColumn<string>(
                    name: "BookingStatus",
                    table: "Bookings",
                    type: "varchar(50)",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "int");

                migrationBuilder.AlterColumn<string>(
                    name: "City",
                    table: "BookingHotels",
                    type: "varchar(50)",
                    nullable: true,
                    oldClrType: typeof(int),
                    oldType: "int",
                    oldNullable: true);
            }

            /// <inheritdoc />
            protected override void Down(MigrationBuilder migrationBuilder)
            {
                migrationBuilder.AlterColumn<int>(
                    name: "Gender",
                    table: "Travelers",
                    type: "int",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "varchar(50)");

                migrationBuilder.AlterColumn<int>(
                    name: "Status",
                    table: "Payments",
                    type: "int",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "varchar(50)");

                migrationBuilder.AlterColumn<int>(
                    name: "PaymentMethod",
                    table: "Payments",
                    type: "int",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "varchar(50)");

                migrationBuilder.AlterColumn<int>(
                    name: "TransportType",
                    table: "InternationalTransports",
                    type: "int",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "varchar(50)");

                migrationBuilder.AlterColumn<int>(
                    name: "DepartureAirport",
                    table: "InternationalTransports",
                    type: "int",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "varchar(50)");

                migrationBuilder.AlterColumn<int>(
                    name: "ArrivalAirport",
                    table: "InternationalTransports",
                    type: "int",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "varchar(50)");

                migrationBuilder.AlterColumn<int>(
                    name: "HotelCity",
                    table: "Hotels",
                    type: "int",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "varchar(50)");

                migrationBuilder.AlterColumn<int>(
                    name: "InternalTransportType",
                    table: "GroundTransports",
                    type: "int",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "varchar(50)");

                migrationBuilder.AlterColumn<int>(
                    name: "TripType",
                    table: "Bookings",
                    type: "int",
                    nullable: true,
                    oldClrType: typeof(string),
                    oldType: "varchar(50)");

                migrationBuilder.AlterColumn<int>(
                    name: "PaymentStatus",
                    table: "Bookings",
                    type: "int",
                    nullable: true,
                    oldClrType: typeof(string),
                    oldType: "varchar(50)",
                    oldNullable: true);

                migrationBuilder.AlterColumn<int>(
                    name: "PaymentMethod",
                    table: "Bookings",
                    type: "int",
                    nullable: true,
                    oldClrType: typeof(string),
                    oldType: "varchar(50)",
                    oldNullable: true);

                migrationBuilder.AlterColumn<int>(
                    name: "BookingStatus",
                    table: "Bookings",
                    type: "int",
                    nullable: false,
                    oldClrType: typeof(string),
                    oldType: "varchar(50)");

                migrationBuilder.AlterColumn<int>(
                    name: "City",
                    table: "BookingHotels",
                    type: "int",
                    nullable: true,
                    oldClrType: typeof(string),
                    oldType: "varchar(50)",
                    oldNullable: true);
            }
        }
    }
