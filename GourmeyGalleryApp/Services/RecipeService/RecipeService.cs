using AutoMapper;
using GourmeyGalleryApp.Interfaces;
using GourmeyGalleryApp.Models.DTOs.Recipe;
using GourmeyGalleryApp.Models.Entities;
using GourmeyGalleryApp.Repositories.RecipeRepository;
using Microsoft.EntityFrameworkCore;


namespace GourmeyGalleryApp.Services.RecipeService
{
    public class RecipeService : IRecipeService
    {
        private readonly IRepository<Recipe> _recipeRepository;
        private readonly IRecipeRepository _recipeCustomRepository;
        private readonly IMapper _mapper;
        public RecipeService(IRepository<Recipe> recipeRepository, IRecipeRepository recipeCustomRepository, IMapper mapper)
        {
            _recipeRepository = recipeRepository;
            _recipeCustomRepository = recipeCustomRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Recipe>> GetAllRecipesAsync(bool? isAdmin)
        {
            var recipes = await _recipeCustomRepository.GetAllRecipesWithDetailsAsync(isAdmin);
            return _mapper.Map<List<Recipe>>(recipes);
        }

        public async Task<Recipe> GetRecipeByIdAsync(int id)
        {
            var recipe = await _recipeCustomRepository.GetRecipeByIdAsync(id);
            if (recipe == null)
            {
                return null;
            }
            return _mapper.Map<Recipe>(recipe);
        }

        public async Task<List<Rating>> GetRatingsByRecipeId(int id)
        {
            var recipe = await _recipeCustomRepository.GetRatingsByRecipeId(id);
            if (recipe == null)
            {
                return null;
            }
            return _mapper.Map<List<Rating>>(recipe);
        }
        public async Task<IEnumerable<Recipe>> GetRecipesByUserIdAsync(string userId)
        {
            return await _recipeCustomRepository.GetRecipesByUserIdAsync(userId);
        }

        public async Task AddRecipeAsync(Recipe recipe, RecipeDto? recipeDto)
        {
            recipe.Slug = GenerateSlug(recipe.Title);
            await _recipeCustomRepository.AddRecipeAsync(recipe, recipeDto);
            await _recipeCustomRepository.SaveChangesAsync();

            // Set the InstructionsId and IngredientsTotalId
            if (recipe.Instructions != null)
            {
                recipe.Instructions.RecipeId = recipe.Id;
                recipe.InstructionsId = recipe.Instructions.Id;
            }
            if (recipe.IngredientsTotal != null)
            {
                recipe.IngredientsTotal.RecipeId = recipe.Id;
                recipe.IngredientsTotalId = recipe.IngredientsTotal.Id;
            }
            if (recipe.InformationTime != null)
            {
                recipe.InformationTime.RecipeId = recipe.Id;
            }
            if (recipe.NutritionFacts != null)
            {
                recipe.NutritionFacts.RecipeId = recipe.Id;
            }

            await _recipeCustomRepository.SaveChangesAsync();
        }

        public async Task UpdateRecipeAsync(Recipe recipe)
        {

            var oldRecipe = await _recipeRepository.GetByIdAsync(recipe.Id);
            recipe.ApplicationUserId = oldRecipe.ApplicationUserId;
            if (recipe.Instructions != null)
                {
                    if (recipe.Instructions.Id == 0) // New Instructions
                    {
                        recipe.Instructions.RecipeId = recipe.Id;
                        //_recipeCustomRepository.Add(recipe.Instructions);
                    }
                    recipe.InstructionsId = recipe.Instructions.Id;
                }

                if (recipe.IngredientsTotal != null)
                {
                    recipe.IngredientsTotal.RecipeId = recipe.Id;
                    recipe.IngredientsTotalId = recipe.IngredientsTotal.Id;
                }

                if (recipe.InformationTime != null)
                {
                    recipe.InformationTime.RecipeId = recipe.Id;
                }

                if (recipe.NutritionFacts != null)
                {
                    recipe.NutritionFacts.RecipeId = recipe.Id;
                }

                _recipeRepository.Update(recipe);
                await _recipeCustomRepository.SaveChangesAsync();  // Ensure SaveChanges is implemented in the Repository
     

        }

        public async Task DeleteRecipeAsync(int id)
        {
                      
                await _recipeCustomRepository.DeleteRecipeAsync(id);             
                await _recipeCustomRepository.SaveChangesAsync();  // Ensure SaveChanges is implemented in the Repository
           
        }

        public async Task<List<Recipe>> GetPopularRecipesAsync(double ratingThreshold, int ratingCountThreshold, int limit)
        {
            return await _recipeCustomRepository.GetPopularRecipesAsync(ratingThreshold, ratingCountThreshold, limit);
        }

        public async Task<List<Recipe>> GetLatestRecipesAsync(int count)
        {
            return await _recipeCustomRepository.GetLatestRecipesAsync(count);
        }


        #region AdminPanel
        public async Task<List<Recipe>> GetPendingRecipesAsync()
        {
            return await _recipeCustomRepository.GetRecipesByStatusAsync(RecipeStatus.Pending);
        }

        public async Task ApproveRecipeAsync(int recipeId)
        {
            var recipe = await _recipeRepository.GetByIdAsync(recipeId);
            if (recipe != null)
            {
                recipe.Status = RecipeStatus.Approved;
                recipe.UpdatedAt = DateTime.UtcNow;
                await _recipeRepository.SaveChangesAsync();
            }
        }

        public async Task RejectRecipeAsync(int recipeId)
        {
            var recipe = await _recipeRepository.GetByIdAsync(recipeId);
            if (recipe != null)
            {
                recipe.Status = RecipeStatus.Rejected;
                recipe.UpdatedAt = DateTime.UtcNow;
                await _recipeRepository.SaveChangesAsync();
            }
        }
        #endregion

        public string GenerateSlug(string title)
        {
            return title.ToLower().Replace(" ", "-").Replace(".", "").Replace("'", "");
        }
    }
}
