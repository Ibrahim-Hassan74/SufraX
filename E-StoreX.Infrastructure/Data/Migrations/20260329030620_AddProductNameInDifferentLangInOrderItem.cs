using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EStoreX.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProductNameInDifferentLangInOrderItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ProductName",
                table: "OrderItems",
                newName: "ProductNameEn");

            migrationBuilder.AddColumn<string>(
                name: "ProductNameAr",
                table: "OrderItems",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProductNameAr",
                table: "OrderItems");

            migrationBuilder.RenameColumn(
                name: "ProductNameEn",
                table: "OrderItems",
                newName: "ProductName");
        }
    }
}
