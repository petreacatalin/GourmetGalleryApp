using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GourmeyGalleryApp.Migrations
{
    /// <inheritdoc />
    public partial class CommentsHelpfulIsEditModif : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsReply",
                table: "Comments",
                newName: "IsEdited");

            migrationBuilder.AddColumn<int>(
                name: "HelpfulCount",
                table: "Comments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NotHelpfulCount",
                table: "Comments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HelpfulCount",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "NotHelpfulCount",
                table: "Comments");

            migrationBuilder.RenameColumn(
                name: "IsEdited",
                table: "Comments",
                newName: "IsReply");
        }
    }
}
