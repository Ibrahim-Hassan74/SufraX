using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EStoreX.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOAuthAndSecurityCallbackUrlsToApiClient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountActivationFailureUrl",
                table: "ApiClients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AccountActivationSuccessUrl",
                table: "ApiClients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OAuthLoginFailureUrl",
                table: "ApiClients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordResetFailureUrl",
                table: "ApiClients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordResetSuccessUrl",
                table: "ApiClients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ApiClients",
                keyColumn: "Id",
                keyValue: new Guid("125e2213-8691-45e9-ab60-d4bfa1367428"),
                columns: new[] { "AccountActivationFailureUrl", "AccountActivationSuccessUrl", "OAuthLoginFailureUrl", "PasswordResetFailureUrl", "PasswordResetSuccessUrl" },
                values: new object[] { null, null, null, null, null });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountActivationFailureUrl",
                table: "ApiClients");

            migrationBuilder.DropColumn(
                name: "AccountActivationSuccessUrl",
                table: "ApiClients");

            migrationBuilder.DropColumn(
                name: "OAuthLoginFailureUrl",
                table: "ApiClients");

            migrationBuilder.DropColumn(
                name: "PasswordResetFailureUrl",
                table: "ApiClients");

            migrationBuilder.DropColumn(
                name: "PasswordResetSuccessUrl",
                table: "ApiClients");
        }
    }
}
