using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GourmeyGalleryApp.Migrations
{
    /// <inheritdoc />
    public partial class CommentsSubmitedUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Timestamp",
                table: "Comments",
                newName: "Updated");

            migrationBuilder.AddColumn<DateTime>(
                name: "Submitted",
                table: "Comments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Submitted",
                table: "Comments");

            migrationBuilder.RenameColumn(
                name: "Updated",
                table: "Comments",
                newName: "Timestamp");
        }
    }
}
