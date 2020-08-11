using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Glaz.Server.Migrations
{
    public partial class MakeManyAttachmentsToOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Attachments_ResponseFileId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Attachments_TargetId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ResponseFileId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_TargetId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ResponseFileId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TargetId",
                table: "Orders");

            migrationBuilder.AddColumn<Guid>(
                name: "OrderId",
                table: "Attachments",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<byte>(
                name: "Platform",
                table: "Attachments",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_OrderId",
                table: "Attachments",
                column: "OrderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_Orders_OrderId",
                table: "Attachments",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_Orders_OrderId",
                table: "Attachments");

            migrationBuilder.DropIndex(
                name: "IX_Attachments_OrderId",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "OrderId",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "Platform",
                table: "Attachments");

            migrationBuilder.AddColumn<Guid>(
                name: "ResponseFileId",
                table: "Orders",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "TargetId",
                table: "Orders",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ResponseFileId",
                table: "Orders",
                column: "ResponseFileId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_TargetId",
                table: "Orders",
                column: "TargetId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Attachments_ResponseFileId",
                table: "Orders",
                column: "ResponseFileId",
                principalTable: "Attachments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Attachments_TargetId",
                table: "Orders",
                column: "TargetId",
                principalTable: "Attachments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
