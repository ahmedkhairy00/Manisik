using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UmarahBooking.Data.Migrations
{
    /// <inheritdoc />
    public partial class addcolReturndateAndRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ReturnDate",
                table: "InternationalTransports",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "rate",
                table: "InternationalTransports",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "review",
                table: "InternationalTransports",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReturnDate",
                table: "InternationalTransports");

            migrationBuilder.DropColumn(
                name: "rate",
                table: "InternationalTransports");

            migrationBuilder.DropColumn(
                name: "review",
                table: "InternationalTransports");
        }
    }
}
