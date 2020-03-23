using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FishingJournal.Migrations
{
    public partial class UserEmailToJournalEntry : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "JournalEntries",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "JournalEntries");
        }
    }
}
