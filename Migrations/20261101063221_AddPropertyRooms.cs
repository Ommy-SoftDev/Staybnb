using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Staybnb.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyRooms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Bathrooms",
                table: "HostProperties",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Bedrooms",
                table: "HostProperties",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Beds",
                table: "HostProperties",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bathrooms",
                table: "HostProperties");

            migrationBuilder.DropColumn(
                name: "Bedrooms",
                table: "HostProperties");

            migrationBuilder.DropColumn(
                name: "Beds",
                table: "HostProperties");
        }
    }
}
