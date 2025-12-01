using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UmarahBooking.Data.Migrations
{
    /// <inheritdoc />
    public partial class updateGroundTransportTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Duration",
                table: "GroundTransports",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Route",
                table: "GroundTransports",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "rate",
                table: "GroundTransports",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Duration",
                table: "GroundTransports");

            migrationBuilder.DropColumn(
                name: "Route",
                table: "GroundTransports");

            migrationBuilder.DropColumn(
                name: "rate",
                table: "GroundTransports");
        }
    }
}
