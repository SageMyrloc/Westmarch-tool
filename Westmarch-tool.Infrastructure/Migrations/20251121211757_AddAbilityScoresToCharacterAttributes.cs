using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Westmarch_tool.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAbilityScoresToCharacterAttributes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Charisma",
                table: "CharacterAttributes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Constitution",
                table: "CharacterAttributes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Dexterity",
                table: "CharacterAttributes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Intelligence",
                table: "CharacterAttributes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Strength",
                table: "CharacterAttributes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Wisdom",
                table: "CharacterAttributes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Charisma",
                table: "CharacterAttributes");

            migrationBuilder.DropColumn(
                name: "Constitution",
                table: "CharacterAttributes");

            migrationBuilder.DropColumn(
                name: "Dexterity",
                table: "CharacterAttributes");

            migrationBuilder.DropColumn(
                name: "Intelligence",
                table: "CharacterAttributes");

            migrationBuilder.DropColumn(
                name: "Strength",
                table: "CharacterAttributes");

            migrationBuilder.DropColumn(
                name: "Wisdom",
                table: "CharacterAttributes");
        }
    }
}
