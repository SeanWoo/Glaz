using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Glaz.Server.Migrations
{
    public partial class UpdateRelationshipsAndEntities : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_OrderStates_StateId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_VuforiaDetails_Attachments_AttachmentId",
                table: "VuforiaDetails");

            migrationBuilder.DropTable(
                name: "OrderStates");

            migrationBuilder.DropIndex(
                name: "IX_VuforiaDetails_AttachmentId",
                table: "VuforiaDetails");

            migrationBuilder.DropIndex(
                name: "IX_Orders_StateId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AttachmentId",
                table: "VuforiaDetails");

            migrationBuilder.DropColumn(
                name: "StateId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "Attachments");

            migrationBuilder.AddColumn<Guid>(
                name: "DetailsId",
                table: "Orders",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ResponseFileId",
                table: "Orders",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "Orders",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "TargetId",
                table: "Orders",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "AccountId",
                table: "Attachments",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Path",
                table: "Attachments",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_DetailsId",
                table: "Orders",
                column: "DetailsId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ResponseFileId",
                table: "Orders",
                column: "ResponseFileId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_TargetId",
                table: "Orders",
                column: "TargetId");

            migrationBuilder.CreateIndex(
                name: "IX_Attachments_AccountId",
                table: "Attachments",
                column: "AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachments_AspNetUsers_AccountId",
                table: "Attachments",
                column: "AccountId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_VuforiaDetails_DetailsId",
                table: "Orders",
                column: "DetailsId",
                principalTable: "VuforiaDetails",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

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

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachments_AspNetUsers_AccountId",
                table: "Attachments");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_VuforiaDetails_DetailsId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Attachments_ResponseFileId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Attachments_TargetId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_DetailsId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_ResponseFileId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_TargetId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Attachments_AccountId",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "DetailsId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ResponseFileId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "State",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TargetId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AccountId",
                table: "Attachments");

            migrationBuilder.DropColumn(
                name: "Path",
                table: "Attachments");

            migrationBuilder.AddColumn<string>(
                name: "AttachmentId",
                table: "VuforiaDetails",
                type: "char(36)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StateId",
                table: "Orders",
                type: "char(36)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "Attachments",
                type: "longtext CHARACTER SET utf8mb4",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OrderStates",
                columns: table => new
                {
                    Id = table.Column<string>(type: "char(36)", nullable: false),
                    Name = table.Column<string>(type: "varchar(128) CHARACTER SET utf8mb4", maxLength: 128, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderStates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VuforiaDetails_AttachmentId",
                table: "VuforiaDetails",
                column: "AttachmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_StateId",
                table: "Orders",
                column: "StateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_OrderStates_StateId",
                table: "Orders",
                column: "StateId",
                principalTable: "OrderStates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_VuforiaDetails_Attachments_AttachmentId",
                table: "VuforiaDetails",
                column: "AttachmentId",
                principalTable: "Attachments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
