using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WhiteLagoon.Web.Migrations
{
    /// <inheritdoc />
    public partial class modify_villa_table : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImgageUrl",
                table: "Villas",
                newName: "ImageUrl");

            migrationBuilder.AddColumn<double>(
                name: "Price",
                table: "Villas",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "Villas");

            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Villas",
                newName: "ImgageUrl");
        }
    }
}
