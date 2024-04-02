using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class chatIdFriendships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChatId",
                table: "Friendships",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_ChatId",
                table: "Friendships",
                column: "ChatId");

            migrationBuilder.AddForeignKey(
                name: "FK_Friendships_Chats_ChatId",
                table: "Friendships",
                column: "ChatId",
                principalTable: "Chats",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Friendships_Chats_ChatId",
                table: "Friendships");

            migrationBuilder.DropIndex(
                name: "IX_Friendships_ChatId",
                table: "Friendships");

            migrationBuilder.DropColumn(
                name: "ChatId",
                table: "Friendships");
        }
    }
}
