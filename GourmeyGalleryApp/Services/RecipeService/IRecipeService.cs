using GourmeyGalleryApp.Models.DTOs.Recipe;
using GourmeyGalleryApp.Models.Entities;

namespace GourmeyGalleryApp.Services.RecipeService
{
    public interface IRecipeService
    {
        Task<IEnumerable<Recipe>> GetAllRecipesAsync(bool? isAdmin);
        Task<Recipe> GetRecipeByIdAsync(int id);
        Task<List<Rating>> GetRatingsByRecipeId(int id);
        Task AddRecipeAsync(Recipe recipe, RecipeDto? recipeDto);
        Task UpdateRecipeAsync(Recipe recipe);
        Task DeleteRecipeAsync(int id);
        Task<IEnumerable<Recipe>> GetRecipesByUserIdAsync(string userId);
        Task<List<Recipe>> GetPopularRecipesAsync(double ratingThreshold, int ratingCountThreshold, int limit);
        Task<List<Recipe>> GetLatestRecipesAsync(int count);

        Task<List<Recipe>> GetPendingRecipesAsync();
        Task ApproveRecipeAsync(int recipeId);
        Task RejectRecipeAsync(int recipeId);


    }
}
