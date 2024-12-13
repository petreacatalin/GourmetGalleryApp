using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GourmeyGalleryApp.Migrations
{
    /// <inheritdoc />
    public partial class UserBadgesfIX : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Icon",
                table: "Badges",
                newName: "IconUrl");

            migrationBuilder.RenameColumn(
                name: "Criteria",
                table: "Badges",
                newName: "Description");

            migrationBuilder.AddColumn<int>(
                name: "Condition",
                table: "Badges",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Badges",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Points",
                table: "Badges",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Condition",
                table: "Badges");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Badges");

            migrationBuilder.DropColumn(
                name: "Points",
                table: "Badges");

            migrationBuilder.RenameColumn(
                name: "IconUrl",
                table: "Badges",
                newName: "Icon");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Badges",
                newName: "Criteria");
        }
    }
}
