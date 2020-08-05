using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Glaz.Server.Migrations
{
    public partial class AddRelationShipsToAttachments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountId",
                table: "Attachments",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AttachmentToOrder",
                columns: table => new
                {
                    AttachmentId = table.Column<Guid>(nullable: false),
                    OrderId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttachmentToOrder", x => new { x.AttachmentId, x.OrderId });
                    table.ForeignKey(
                        name: "FK_AttachmentToOrder_Attachments_AttachmentId",
                        column: x => x.AttachmentId,
                        principalTable: "Attachments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AttachmentToOrder_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_AccountId",
                table: "Attachments",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AttachmentToOrder_OrderId",
                table: "AttachmentToOrder",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_AspNetUsers_AccountId",
                table: "Attachments",
                column: "AccountId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_AspNetUsers_AccountId",
                table: "Attachments");

            migrationBuilder.DropTable(
                name: "AttachmentToOrder");

            migrationBuilder.DropIndex(
                name: "IX_Attachments_AccountId",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "Attachments");
        }
    }
}
