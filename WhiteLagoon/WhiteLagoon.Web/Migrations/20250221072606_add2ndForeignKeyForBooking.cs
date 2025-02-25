using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhiteLagoon.Web.Migrations
{
    /// <inheritdoc />
    public partial class add2ndForeignKeyForBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "VillaId",
                table: "Bookings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Bookings_VillaId",
                table: "Bookings",
                column: "VillaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_Villas_VillaId",
                table: "Bookings",
                column: "VillaId",
                principalTable: "Villas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_Villas_VillaId",
                table: "Bookings");

            migrationBuilder.DropIndex(
                name: "IX_Bookings_VillaId",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "VillaId",
                table: "Bookings");
        }
    }
}
