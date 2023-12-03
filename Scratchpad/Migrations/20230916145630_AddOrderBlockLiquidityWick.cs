using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Scratchpad.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderBlockLiquidityWick : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "LiquidityWick",
                table: "OrderBlocks",
                type: "TEXT",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LiquidityWick",
                table: "OrderBlocks");
        }
    }
}
