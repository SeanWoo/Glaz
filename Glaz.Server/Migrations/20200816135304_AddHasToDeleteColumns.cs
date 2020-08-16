using Microsoft.EntityFrameworkCore.Migrations;

namespace Glaz.Server.Migrations
{
    public partial class AddHasToDeleteColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasToDelete",
                table: "VuforiaDetails",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HastToDelete",
                table: "Orders",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HastToDelete",
                table: "Attachments",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasToDelete",
                table: "VuforiaDetails");

            migrationBuilder.DropColumn(
                name: "HastToDelete",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "HastToDelete",
                table: "Attachments");
        }
    }
}
