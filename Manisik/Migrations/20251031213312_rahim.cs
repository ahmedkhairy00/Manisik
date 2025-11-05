using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Manisik.Migrations
{
    /// <inheritdoc />
    public partial class rahim : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UmrahBookings_Hotels_HotelId",
                table: "UmrahBookings");

            migrationBuilder.DropIndex(
                name: "IX_UmrahBookings_HotelId",
                table: "UmrahBookings");

            migrationBuilder.DropColumn(
                name: "HotelId",
                table: "UmrahBookings");

            migrationBuilder.RenameColumn(
                name: "Mode",
                table: "UmrahBookings",
                newName: "TravelMode");

            migrationBuilder.RenameColumn(
                name: "AirlineOrShipName",
                table: "UmrahBookings",
                newName: "ShipName");

            migrationBuilder.CreateTable(
                name: "UmrahBookingHotels",
                columns: table => new
                {
                    UmrahBookingHotelId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UmrahBookingId = table.Column<int>(type: "int", nullable: false),
                    HotelId = table.Column<int>(type: "int", nullable: false),
                    CheckIn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckOut = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UmrahBookingHotels", x => x.UmrahBookingHotelId);
                    table.ForeignKey(
                        name: "FK_UmrahBookingHotels_Hotels_HotelId",
                        column: x => x.HotelId,
                        principalTable: "Hotels",
                        principalColumn: "HotelId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UmrahBookingHotels_UmrahBookings_UmrahBookingId",
                        column: x => x.UmrahBookingId,
                        principalTable: "UmrahBookings",
                        principalColumn: "UmrahBookingId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UmrahBookingHotels_HotelId",
                table: "UmrahBookingHotels",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_UmrahBookingHotels_UmrahBookingId",
                table: "UmrahBookingHotels",
                column: "UmrahBookingId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UmrahBookingHotels");

            migrationBuilder.RenameColumn(
                name: "TravelMode",
                table: "UmrahBookings",
                newName: "Mode");

            migrationBuilder.RenameColumn(
                name: "ShipName",
                table: "UmrahBookings",
                newName: "AirlineOrShipName");

            migrationBuilder.AddColumn<int>(
                name: "HotelId",
                table: "UmrahBookings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UmrahBookings_HotelId",
                table: "UmrahBookings",
                column: "HotelId");

            migrationBuilder.AddForeignKey(
                name: "FK_UmrahBookings_Hotels_HotelId",
                table: "UmrahBookings",
                column: "HotelId",
                principalTable: "Hotels",
                principalColumn: "HotelId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
