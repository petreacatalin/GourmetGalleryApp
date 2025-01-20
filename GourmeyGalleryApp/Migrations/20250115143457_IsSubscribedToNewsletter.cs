using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GourmeyGalleryApp.Migrations
{
    /// <inheritdoc />
    public partial class IsSubscribedToNewsletter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Recipes_MealPlans_MealPlanId",
                table: "Recipes");

            migrationBuilder.DropIndex(
                name: "IX_Recipes_MealPlanId",
                table: "Recipes");

            migrationBuilder.DropColumn(
                name: "MealPlanId",
                table: "Recipes");

            migrationBuilder.AddColumn<DateTime>(
                name: "Date",
                table: "MealPlans",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "MealType",
                table: "MealPlans",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "RecipeId",
                table: "MealPlans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsSubscribedToNewsletter",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_MealPlans_RecipeId",
                table: "MealPlans",
                column: "RecipeId");

            migrationBuilder.AddForeignKey(
                name: "FK_MealPlans_Recipes_RecipeId",
                table: "MealPlans",
                column: "RecipeId",
                principalTable: "Recipes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MealPlans_Recipes_RecipeId",
                table: "MealPlans");

            migrationBuilder.DropIndex(
                name: "IX_MealPlans_RecipeId",
                table: "MealPlans");

            migrationBuilder.DropColumn(
                name: "Date",
                table: "MealPlans");

            migrationBuilder.DropColumn(
                name: "MealType",
                table: "MealPlans");

            migrationBuilder.DropColumn(
                name: "RecipeId",
                table: "MealPlans");

            migrationBuilder.DropColumn(
                name: "IsSubscribedToNewsletter",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<int>(
                name: "MealPlanId",
                table: "Recipes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Recipes_MealPlanId",
                table: "Recipes",
                column: "MealPlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_Recipes_MealPlans_MealPlanId",
                table: "Recipes",
                column: "MealPlanId",
                principalTable: "MealPlans",
                principalColumn: "Id");
        }
    }
}
