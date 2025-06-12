using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotesApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchemaSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "dev");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "Users",
                newSchema: "dev");

            migrationBuilder.RenameTable(
                name: "RefreshTokens",
                newName: "RefreshTokens",
                newSchema: "dev");

            migrationBuilder.RenameTable(
                name: "Notes",
                newName: "Notes",
                newSchema: "dev");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Users",
                schema: "dev",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "RefreshTokens",
                schema: "dev",
                newName: "RefreshTokens");

            migrationBuilder.RenameTable(
                name: "Notes",
                schema: "dev",
                newName: "Notes");
        }
    }
}
