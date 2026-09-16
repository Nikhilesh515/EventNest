using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventNest.EventService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueEventTitleIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Events_Title",
                table: "Events",
                column: "Title",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Events_Title",
                table: "Events");
        }
    }
}
