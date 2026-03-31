using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EStoreX.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeArabicRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // =========================
            // 🔁 RENAME (IMPORTANT)
            // =========================

            // DeliveryMethods
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "DeliveryMethods",
                newName: "NameEn");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "DeliveryMethods",
                newName: "DescriptionEn");

            // Products
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Products",
                newName: "NameEn");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Products",
                newName: "DescriptionEn");

            // Categories
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Categories",
                newName: "NameEn");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Categories",
                newName: "DescriptionEn");

            // Brands
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Brands",
                newName: "NameEn");


            // =========================
            // ➕ ADD ARABIC COLUMNS
            // =========================

            // Products
            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "Products",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "Products",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            // DeliveryMethods
            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "DeliveryMethods",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "DeliveryMethods",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            // Categories
            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "Categories",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "Categories",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            // Brands
            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "Brands",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");


            // =========================
            // 🧾 SEED UPDATE (AFTER FIX)
            // =========================

            //migrationBuilder.UpdateData(
            //    table: "DeliveryMethods",
            //    keyColumn: "Id",
            //    keyValue: new Guid("6f2d385c-9b0b-4f93-aaf6-3c62d6c1d333"),
            //    columns: new[] { "DescriptionAr", "DescriptionEn", "NameAr", "NameEn" },
            //    values: new object[] { "توصيل اقتصادي خلال 6-8 يوم", "Economy delivery within 6-8 days", "اقتصادي", "Economy" });

            //migrationBuilder.UpdateData(
            //    table: "DeliveryMethods",
            //    keyColumn: "Id",
            //    keyValue: new Guid("d9372a1e-e6cb-4d1a-9476-1f52f9c8c222"),
            //    columns: new[] { "DescriptionAr", "DescriptionEn", "NameAr", "NameEn" },
            //    values: new object[] { "توصيل عادي خلال 3-5 يوم", "Standard delivery within 3-5 days", "عادي", "Standard" });

            //migrationBuilder.UpdateData(
            //    table: "DeliveryMethods",
            //    keyColumn: "Id",
            //    keyValue: new Guid("f5c2a7b1-4e0e-4a66-8c7a-7c9d50f9e111"),
            //    columns: new[] { "DescriptionAr", "DescriptionEn", "NameAr", "NameEn" },
            //    values: new object[] { "توصيل سريع خلال 1-2 يوم", "Fast delivery within 1-2 days", "سريع", "Fast" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop Arabic columns (reverse order)
            migrationBuilder.DropColumn("NameAr", "Products");
            migrationBuilder.DropColumn("DescriptionAr", "Products");

            migrationBuilder.DropColumn("NameAr", "DeliveryMethods");
            migrationBuilder.DropColumn("DescriptionAr", "DeliveryMethods");

            migrationBuilder.DropColumn("NameAr", "Categories");
            migrationBuilder.DropColumn("DescriptionAr", "Categories");

            migrationBuilder.DropColumn("NameAr", "Brands");

            // Rename back
            migrationBuilder.RenameColumn("NameEn", "Products", "Name");
            migrationBuilder.RenameColumn("DescriptionEn", "Products", "Description");

            migrationBuilder.RenameColumn("NameEn", "DeliveryMethods", "Name");
            migrationBuilder.RenameColumn("DescriptionEn", "DeliveryMethods", "Description");

            migrationBuilder.RenameColumn("NameEn", "Categories", "Name");
            migrationBuilder.RenameColumn("DescriptionEn", "Categories", "Description");

            migrationBuilder.RenameColumn("NameEn", "Brands", "Name");
        }
    }
}
