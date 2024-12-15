using GourmeyGalleryApp.Models.DTOs.Recipe;
using GourmeyGalleryApp.Models.Entities;
using GourmeyGalleryApp.Repositories.UserRepo;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GourmetGallery.Infrastructure.Repositories
{
    public class UserRepository : Repository<ApplicationUser>, IUserRepository
    {
        private readonly GourmetGalleryContext _context;

        public UserRepository(GourmetGalleryContext context) : base(context)
        {
            _context = context;
        }

        public async Task<ApplicationUser> GetUserByIdAsync(string userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        public async Task<IEnumerable<ApplicationUser>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        // Implement favorite methods
        public async Task AddToFavoritesAsync(string userId, int recipeId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            var recipe = await _context.Recipes.FindAsync(recipeId);
            if (recipe == null)
            {
                throw new KeyNotFoundException("Recipe not found.");
            }

            var favorite = new UserFavoriteRecipe
            {
                UserId = userId,
                RecipeId = recipeId
            };

            _context.UserFavoriteRecipes.Add(favorite);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveFromFavoritesAsync(string userId, int recipeId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            var favorite = await _context.UserFavoriteRecipes
                .FirstOrDefaultAsync(uf => uf.UserId == userId && uf.RecipeId == recipeId);

            if (favorite == null)
            {
                throw new KeyNotFoundException("Favorite not found.");
            }

            _context.UserFavoriteRecipes.Remove(favorite);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<RecipeDto>> GetFavoriteRecipesAsync(string userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            var favoriteRecipes = await _context.UserFavoriteRecipes
                .AsNoTracking()
                .Where(uf => uf.UserId == userId)
                .Select(uf => uf.Recipe)
                .ToListAsync();

            var recipeDtos = favoriteRecipes.Select(recipe => new RecipeDto
            {
                // Map properties of Recipe to RecipeDto
                Id = recipe.Id,
                Title = recipe.Title,
                Description = recipe.Description,
                ImageUrl = recipe.ImageUrl,
                Status = recipe.Status,
                Slug = recipe.Slug
                // Add other properties as needed
            });

            return recipeDtos;
        }
    }
}
