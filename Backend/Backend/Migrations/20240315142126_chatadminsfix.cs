using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class chatadminsfix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatAdmin_AspNetUsers_UserId",
                table: "ChatAdmin");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatAdmin_Chats_ChatId",
                table: "ChatAdmin");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChatAdmin",
                table: "ChatAdmin");

            migrationBuilder.RenameTable(
                name: "ChatAdmin",
                newName: "ChatAdmins");

            migrationBuilder.RenameIndex(
                name: "IX_ChatAdmin_UserId",
                table: "ChatAdmins",
                newName: "IX_ChatAdmins_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatAdmins",
                table: "ChatAdmins",
                columns: new[] { "ChatId", "UserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ChatAdmins_AspNetUsers_UserId",
                table: "ChatAdmins",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatAdmins_Chats_ChatId",
                table: "ChatAdmins",
                column: "ChatId",
                principalTable: "Chats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatAdmins_AspNetUsers_UserId",
                table: "ChatAdmins");

            migrationBuilder.DropForeignKey(
                name: "FK_ChatAdmins_Chats_ChatId",
                table: "ChatAdmins");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChatAdmins",
                table: "ChatAdmins");

            migrationBuilder.RenameTable(
                name: "ChatAdmins",
                newName: "ChatAdmin");

            migrationBuilder.RenameIndex(
                name: "IX_ChatAdmins_UserId",
                table: "ChatAdmin",
                newName: "IX_ChatAdmin_UserId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatAdmin",
                table: "ChatAdmin",
                columns: new[] { "ChatId", "UserId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ChatAdmin_AspNetUsers_UserId",
                table: "ChatAdmin",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatAdmin_Chats_ChatId",
                table: "ChatAdmin",
                column: "ChatId",
                principalTable: "Chats",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
