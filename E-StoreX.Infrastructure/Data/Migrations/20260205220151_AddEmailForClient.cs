using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EStoreX.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailForClient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "ApiClients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ApiClients",
                keyColumn: "Id",
                keyValue: new Guid("125e2213-8691-45e9-ab60-d4bfa1367428"),
                column: "Email",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Email",
                table: "ApiClients");
        }
    }
}
