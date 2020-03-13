using Microsoft.EntityFrameworkCore.Migrations;

namespace FishingJournal.Migrations
{
    public partial class AddLatLongToJournalEntry : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Location",
                table: "JournalEntries");

            migrationBuilder.AddColumn<string>(
                name: "Latitude",
                table: "JournalEntries",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Longitude",
                table: "JournalEntries",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "JournalEntries");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "JournalEntries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
