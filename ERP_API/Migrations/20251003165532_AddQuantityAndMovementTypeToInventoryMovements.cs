using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ERP_API.Migrations
{
    /// <inheritdoc />
    public partial class AddQuantityAndMovementTypeToInventoryMovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "QuantityChange",
                table: "InventoryMovements",
                newName: "Quantity");

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "InventoryMovements",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "MovementType",
                table: "InventoryMovements",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MovementType",
                table: "InventoryMovements");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "InventoryMovements",
                newName: "QuantityChange");

            migrationBuilder.AlterColumn<string>(
                name: "Reason",
                table: "InventoryMovements",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
