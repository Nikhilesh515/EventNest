using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventNest.AuthService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefreshTokenHardening : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_refresh_tokens_token",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "is_revoked",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "revoked_by_ip",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "token",
                table: "refresh_tokens");

            // Clear existing tokens — they used plaintext storage and are invalid after schema change
            migrationBuilder.Sql("DELETE FROM refresh_tokens;");

            migrationBuilder.AlterColumn<string>(
                name: "created_by_ip",
                table: "refresh_tokens",
                type: "character varying(45)",
                maxLength: 45,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(45)",
                oldMaxLength: 45);

            migrationBuilder.AddColumn<string>(
                name: "replaced_by_token_hash",
                table: "refresh_tokens",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "revoked_at",
                table: "refresh_tokens",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "token_hash",
                table: "refresh_tokens",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "idx_refresh_tokens_token_hash",
                table: "refresh_tokens",
                column: "token_hash",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_refresh_tokens_token_hash",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "replaced_by_token_hash",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "revoked_at",
                table: "refresh_tokens");

            migrationBuilder.DropColumn(
                name: "token_hash",
                table: "refresh_tokens");

            migrationBuilder.AlterColumn<string>(
                name: "created_by_ip",
                table: "refresh_tokens",
                type: "character varying(45)",
                maxLength: 45,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(45)",
                oldMaxLength: 45,
                oldNullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_revoked",
                table: "refresh_tokens",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "revoked_by_ip",
                table: "refresh_tokens",
                type: "character varying(45)",
                maxLength: 45,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "token",
                table: "refresh_tokens",
                type: "character varying(512)",
                maxLength: 512,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "idx_refresh_tokens_token",
                table: "refresh_tokens",
                column: "token");
        }
    }
}
