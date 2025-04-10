using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Core.DAL.Migrations
{
    /// <inheritdoc />
    public partial class CloseDateTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CloseDateTime",
                table: "BankAccounts",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CloseDateTime",
                table: "BankAccounts");
        }
    }
}
