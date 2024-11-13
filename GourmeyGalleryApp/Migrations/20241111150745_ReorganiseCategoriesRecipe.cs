using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GourmeyGalleryApp.Migrations
{
    /// <inheritdoc />
    public partial class ReorganiseCategoriesRecipe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CookingMethod",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "Cuisine",
                table: "Recipes");

            migrationBuilder.RenameColumn(
                name: "OtherCategories",
                table: "Recipes",
                newName: "OccasionId");

            migrationBuilder.RenameColumn(
                name: "Occasion",
                table: "Recipes",
                newName: "MealTypeId");

            migrationBuilder.RenameColumn(
                name: "MealType",
                table: "Recipes",
                newName: "IngredientId");

            migrationBuilder.RenameColumn(
                name: "MainIngredient",
                table: "Recipes",
                newName: "CuisineId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "OccasionId",
                table: "Recipes",
                newName: "OtherCategories");

            migrationBuilder.RenameColumn(
                name: "MealTypeId",
                table: "Recipes",
                newName: "Occasion");

            migrationBuilder.RenameColumn(
                name: "IngredientId",
                table: "Recipes",
                newName: "MealType");

            migrationBuilder.RenameColumn(
                name: "CuisineId",
                table: "Recipes",
                newName: "MainIngredient");

            migrationBuilder.AddColumn<int>(
                name: "CookingMethod",
                table: "Recipes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Cuisine",
                table: "Recipes",
                type: "int",
                nullable: true);
        }
    }
}
