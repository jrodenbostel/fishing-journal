using Microsoft.EntityFrameworkCore.Migrations;

namespace FishingJournal.Migrations
{
    public partial class UpdateJournalEntry : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WeatherSummary",
                table: "JournalEntries");

            migrationBuilder.AddColumn<string>(
                name: "BarometricPressure",
                table: "JournalEntries",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Humidity",
                table: "JournalEntries",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Precipitation",
                table: "JournalEntries",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Temperature",
                table: "JournalEntries",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WindDirection",
                table: "JournalEntries",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WindSpeed",
                table: "JournalEntries",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BarometricPressure",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "Humidity",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "Precipitation",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "Temperature",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "WindDirection",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "WindSpeed",
                table: "JournalEntries");

            migrationBuilder.AddColumn<string>(
                name: "WeatherSummary",
                table: "JournalEntries",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
