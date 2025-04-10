using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Core.DAL.Migrations
{
    /// <inheritdoc />
    public partial class CurrencyType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CurrencyTypeId",
                table: "BankAccounts",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Vname = table.Column<string>(type: "text", nullable: false),
                    Vnom = table.Column<int>(type: "integer", nullable: false),
                    Vcurs = table.Column<decimal>(type: "numeric", nullable: false),
                    Vcode = table.Column<int>(type: "integer", nullable: false),
                    VchCode = table.Column<string>(type: "text", nullable: false),
                    VunitRate = table.Column<decimal>(type: "numeric", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifyDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_CurrencyTypeId",
                table: "BankAccounts",
                column: "CurrencyTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_Vcode",
                table: "Currencies",
                column: "Vcode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_Vname",
                table: "Currencies",
                column: "Vname");

            migrationBuilder.AddForeignKey(
                name: "FK_BankAccounts_Currencies_CurrencyTypeId",
                table: "BankAccounts",
                column: "CurrencyTypeId",
                principalTable: "Currencies",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankAccounts_Currencies_CurrencyTypeId",
                table: "BankAccounts");

            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.DropIndex(
                name: "IX_BankAccounts_CurrencyTypeId",
                table: "BankAccounts");

            migrationBuilder.DropColumn(
                name: "CurrencyTypeId",
                table: "BankAccounts");
        }
    }
}
