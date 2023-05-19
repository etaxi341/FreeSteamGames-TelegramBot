using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataManager.Migrations
{
    public partial class AddIndices : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_subscribers_chatID",
                table: "subscribers",
                column: "chatID");

            migrationBuilder.CreateIndex(
                name: "IX_notifications_chatID_steamLink",
                table: "notifications",
                columns: new[] { "chatID", "steamLink" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_subscribers_chatID",
                table: "subscribers");

            migrationBuilder.DropIndex(
                name: "IX_notifications_chatID_steamLink",
                table: "notifications");
        }
    }
}
