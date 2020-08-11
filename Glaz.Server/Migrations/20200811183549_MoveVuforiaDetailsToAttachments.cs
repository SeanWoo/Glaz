using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Glaz.Server.Migrations
{
    public partial class MoveVuforiaDetailsToAttachments : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_VuforiaDetails_DetailsId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_DetailsId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "DetailsId",
                table: "Orders");

            migrationBuilder.AddColumn<Guid>(
                name: "AttachmentId",
                table: "VuforiaDetails",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_VuforiaDetails_AttachmentId",
                table: "VuforiaDetails",
                column: "AttachmentId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_VuforiaDetails_Attachments_AttachmentId",
                table: "VuforiaDetails",
                column: "AttachmentId",
                principalTable: "Attachments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VuforiaDetails_Attachments_AttachmentId",
                table: "VuforiaDetails");

            migrationBuilder.DropIndex(
                name: "IX_VuforiaDetails_AttachmentId",
                table: "VuforiaDetails");

            migrationBuilder.DropColumn(
                name: "AttachmentId",
                table: "VuforiaDetails");

            migrationBuilder.AddColumn<Guid>(
                name: "DetailsId",
                table: "Orders",
                type: "char(36)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_DetailsId",
                table: "Orders",
                column: "DetailsId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_VuforiaDetails_DetailsId",
                table: "Orders",
                column: "DetailsId",
                principalTable: "VuforiaDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
