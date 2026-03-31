using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EStoreX.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Email",
                table: "Orders",
                newName: "BuyerEmail");

            //migrationBuilder.InsertData(
            //    table: "DeliveryMethods",
            //    columns: new[] { "Id", "DeliveryTime", "Description", "Name", "Price" },
            //    values: new object[,]
            //    {
            //        { new Guid("6f2d385c-9b0b-4f93-aaf6-3c62d6c1d333"), "6-8 Days", "Economy delivery within 6-8 days", "Economy", 10m },
            //        { new Guid("d9372a1e-e6cb-4d1a-9476-1f52f9c8c222"), "3-5 Days", "Standard delivery within 3-5 days", "Standard", 20m },
            //        { new Guid("f5c2a7b1-4e0e-4a66-8c7a-7c9d50f9e111"), "1-2 Days", "Fast delivery within 1-2 days", "Fast", 50m }
            //    });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DeleteData(
            //    table: "DeliveryMethods",
            //    keyColumn: "Id",
            //    keyValue: new Guid("6f2d385c-9b0b-4f93-aaf6-3c62d6c1d333"));

            //migrationBuilder.DeleteData(
            //    table: "DeliveryMethods",
            //    keyColumn: "Id",
            //    keyValue: new Guid("d9372a1e-e6cb-4d1a-9476-1f52f9c8c222"));

            //migrationBuilder.DeleteData(
            //    table: "DeliveryMethods",
            //    keyColumn: "Id",
            //    keyValue: new Guid("f5c2a7b1-4e0e-4a66-8c7a-7c9d50f9e111"));

            migrationBuilder.RenameColumn(
                name: "BuyerEmail",
                table: "Orders",
                newName: "Email");
        }
    }
}
