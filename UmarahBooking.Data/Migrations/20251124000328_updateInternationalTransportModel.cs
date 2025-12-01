using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UmarahBooking.Data.Migrations
{
    /// <inheritdoc />
    public partial class updateInternationalTransportModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Duration",
                table: "InternationalTransports",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "FlightClass",
                table: "InternationalTransports",
                type: "int",
                maxLength: 50,
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Stops",
                table: "InternationalTransports",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Duration",
                table: "InternationalTransports");

            migrationBuilder.DropColumn(
                name: "FlightClass",
                table: "InternationalTransports");

            migrationBuilder.DropColumn(
                name: "Stops",
                table: "InternationalTransports");
        }
    }
}
