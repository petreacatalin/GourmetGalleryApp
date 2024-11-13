using GourmeyGalleryApp.Models.DTOs.Recipe;
using GourmeyGalleryApp.Models.Entities;

namespace GourmeyGalleryApp.Repositories.RecipeRepository
{
    public interface IRecipeRepository
    {
        Task AddRecipeAsync(Recipe recipe, RecipeDto recipeDto);
        Task<List<Recipe>> GetAllRecipesWithDetailsAsync(bool? isAdmin);
        Task<Recipe> GetRecipeByIdAsync(int id);
        Task<List<Rating>> GetRatingsByRecipeId(int id);
        Task SaveChangesAsync();
        Task<List<Recipe>> GetRecipesByUserIdAsync(string userId);
        Task<List<Recipe>> GetPopularRecipesAsync(double ratingThreshold, int ratingCountThreshold, int limit);
        Task<List<Recipe>> GetLatestRecipesAsync(int count);
        Task<List<Recipe>> GetRecipesByStatusAsync(RecipeStatus status);
        Task DeleteRecipeAsync(int recipeId);

    }

}
