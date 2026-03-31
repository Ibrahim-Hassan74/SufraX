using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EStoreX.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UnifyApiClientCallbackUrls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // OAuth
            //migrationBuilder.RenameColumn(
            //    name: "OAuthLoginSuccessUrl",
            //    table: "ApiClients",
            //    newName: "OAuthCallbackUrl");

            migrationBuilder.Sql(@"
            IF EXISTS (
                SELECT 1 FROM sys.columns 
                WHERE Name = 'OAuthLoginSuccessUrl'
                AND Object_ID = Object_ID('ApiClients')
            )
            BEGIN
                EXEC sp_rename 'ApiClients.OAuthLoginSuccessUrl', 'OAuthCallbackUrl', 'COLUMN';
            END
            ");

            migrationBuilder.DropColumn(
                name: "OAuthLoginFailureUrl",
                table: "ApiClients");

            // Account Activation
            migrationBuilder.RenameColumn(
                name: "AccountActivationSuccessUrl",
                table: "ApiClients",
                newName: "AccountActivationCallbackUrl");

            migrationBuilder.DropColumn(
                name: "AccountActivationFailureUrl",
                table: "ApiClients");

            // Password Reset
            migrationBuilder.RenameColumn(
                name: "PasswordResetSuccessUrl",
                table: "ApiClients",
                newName: "PasswordResetCallbackUrl");

            migrationBuilder.DropColumn(
                name: "PasswordResetFailureUrl",
                table: "ApiClients");
        }


        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Password Reset
            migrationBuilder.RenameColumn(
                name: "PasswordResetCallbackUrl",
                table: "ApiClients",
                newName: "PasswordResetSuccessUrl");

            migrationBuilder.AddColumn<string>(
                name: "PasswordResetFailureUrl",
                table: "ApiClients",
                type: "nvarchar(max)",
                nullable: true);

            // Account Activation
            migrationBuilder.RenameColumn(
                name: "AccountActivationCallbackUrl",
                table: "ApiClients",
                newName: "AccountActivationSuccessUrl");

            migrationBuilder.AddColumn<string>(
                name: "AccountActivationFailureUrl",
                table: "ApiClients",
                type: "nvarchar(max)",
                nullable: true);

            // OAuth
            migrationBuilder.RenameColumn(
                name: "OAuthCallbackUrl",
                table: "ApiClients",
                newName: "OAuthLoginSuccessUrl");

            migrationBuilder.AddColumn<string>(
                name: "OAuthLoginFailureUrl",
                table: "ApiClients",
                type: "nvarchar(max)",
                nullable: true);
        }

    }
}
