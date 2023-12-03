using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scratchpad.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderBlocks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Open",
                table: "Candles",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<decimal>(
                name: "Low",
                table: "Candles",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<decimal>(
                name: "High",
                table: "Candles",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<decimal>(
                name: "Close",
                table: "Candles",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "TEXT");

            migrationBuilder.CreateTable(
                name: "OrderBlocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Symbol = table.Column<string>(type: "TEXT", nullable: false),
                    Open = table.Column<decimal>(type: "REAL", nullable: false),
                    High = table.Column<decimal>(type: "REAL", nullable: false),
                    Low = table.Column<decimal>(type: "REAL", nullable: false),
                    Close = table.Column<decimal>(type: "REAL", nullable: false),
                    OpenDateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CloseDateTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderBlocks", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Candles_CloseDateTime",
                table: "Candles",
                column: "CloseDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_Candles_OpenDateTime",
                table: "Candles",
                column: "OpenDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_Candles_Symbol",
                table: "Candles",
                column: "Symbol");

            migrationBuilder.CreateIndex(
                name: "IX_OrderBlocks_CloseDateTime",
                table: "OrderBlocks",
                column: "CloseDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_OrderBlocks_OpenDateTime",
                table: "OrderBlocks",
                column: "OpenDateTime");

            migrationBuilder.CreateIndex(
                name: "IX_OrderBlocks_Symbol",
                table: "OrderBlocks",
                column: "Symbol");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrderBlocks");

            migrationBuilder.DropIndex(
                name: "IX_Candles_CloseDateTime",
                table: "Candles");

            migrationBuilder.DropIndex(
                name: "IX_Candles_OpenDateTime",
                table: "Candles");

            migrationBuilder.DropIndex(
                name: "IX_Candles_Symbol",
                table: "Candles");

            migrationBuilder.AlterColumn<decimal>(
                name: "Open",
                table: "Candles",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "REAL");

            migrationBuilder.AlterColumn<decimal>(
                name: "Low",
                table: "Candles",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "REAL");

            migrationBuilder.AlterColumn<decimal>(
                name: "High",
                table: "Candles",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "REAL");

            migrationBuilder.AlterColumn<decimal>(
                name: "Close",
                table: "Candles",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "REAL");
        }
    }
}
