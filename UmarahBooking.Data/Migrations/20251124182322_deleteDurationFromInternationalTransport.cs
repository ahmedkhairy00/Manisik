using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UmarahBooking.Data.Migrations
{
    /// <inheritdoc />
    public partial class deleteDurationFromInternationalTransport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Duration",
                table: "InternationalTransports");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Duration",
                table: "InternationalTransports",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
