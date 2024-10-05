﻿using GourmetGallery.Infrastructure;
using GourmeyGalleryApp.Models.DTOs.Comments;
using GourmeyGalleryApp.Models.DTOs.Recipe;
using GourmeyGalleryApp.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace GourmeyGalleryApp.Repositories.RecipeRepository
{
    public class RecipeRepository : IRecipeRepository
    {
        private readonly GourmetGalleryContext _context;

        public RecipeRepository(GourmetGalleryContext context)
        {
            _context = context;
        }

        public async Task AddRecipeAsync(Recipe recipe)
        {
            // Add the Recipe to the context
            await _context.Set<Recipe>().AddAsync(recipe);

            // Save changes to get the Recipe ID
            await _context.SaveChangesAsync();

        }

        public async Task DeleteRecipeAsync(int recipeId)
        {
            // Get all ratings associated with the recipe
            var ratings = await _context.Ratings
                                        .Where(r => r.RecipeId == recipeId)
                                        .ToListAsync();
            var comments = await _context.Comments
                                        .Where(r => r.RecipeId == recipeId)
                                        .ToListAsync();
            // Remove the ratings first
            if (ratings.Any())
            {
                _context.Ratings.RemoveRange(ratings);
            }
            if (comments.Any())
            {
                _context.Comments.RemoveRange(comments);
            }
            // Now, remove the recipe
            var recipe = await _context.Recipes.FindAsync(recipeId);
            if (recipe != null)
            {
                _context.Recipes.Remove(recipe);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Recipe>> GetAllRecipesWithDetailsAsync(bool? isAdmin)
        {
            return await _context.Recipes
                .Include(r => r.IngredientsTotal)
                    .ThenInclude(it => it.Ingredients)
                .Include(r => r.Instructions)
                    .ThenInclude(i => i.Steps)
                .Include(r => r.Comments)
                    .ThenInclude(rt => rt.Rating)
                .Include(us => us.ApplicationUser)
                .Where(st => isAdmin == true || st.Status == RecipeStatus.Approved) 
                .ToListAsync();
        }

        public async Task<Recipe> GetRecipeByIdAsync(int id)
        {
            return await _context.Recipes
           .Include(r => r.IngredientsTotal)
               .ThenInclude(it => it.Ingredients)
           .Include(r => r.Instructions)
               .ThenInclude(i => i.Steps)
           .Include(r => r.Comments)
           .Include(x=> x.NutritionFacts)
           .Include(x => x.InformationTime)
           .Include(us => us.ApplicationUser)              
           .FirstOrDefaultAsync(r => r.Id == id);
        }
        public async Task<List<Rating>> GetRatingsByRecipeId(int id)
        {
            return await _context.Ratings.Where(x => x.RecipeId == id)
                .Include(x=> x.User)
                .Include(x=> x.Recipe)
                .ToListAsync();
        }

        public async Task<List<Recipe>> GetRecipesByUserIdAsync(string userId)
        {
            return await _context.Recipes
                                 .Where(r => r.ApplicationUserId == userId)  // Assuming you have a UserId property in Recipe
                                 .ToListAsync();
        }

        public async Task<List<Recipe>> GetPopularRecipesAsync(double ratingThreshold, int ratingCountThreshold, int limit)
        {
            // Fetch recipes along with their comments
            var recipes = await _context.Recipes
                .Include(x => x.InformationTime)
                .Include(r => r.Comments)
                    .ThenInclude(c => c.Rating) 
                .ToListAsync(); 

            // Calculate average rating and number of ratings
            var popularRecipes = recipes
                .Select(r => new Recipe
                {
                    Id = r.Id,
                    Title = r.Title,
                    Description = r.Description,
                    ImageUrl = r.ImageUrl,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt,
                    Slug = r.Slug,
                    InformationTime = r.InformationTime != null ? new InformationTime
                    {
                        Id = r.InformationTime.Id,
                        PrepTime = r.InformationTime.PrepTime,
                        CookTime = r.InformationTime.CookTime,
                        StandTime = r.InformationTime.StandTime,
                        TotalTime = r.InformationTime.TotalTime,
                        Servings = r.InformationTime.Servings
                    } : null,
                    Comments = r.Comments.Select(c => new Comment
                    {
                        Rating = c.Rating
                    }).ToList(),
                })
                .Where(r => r.AverageRating >= ratingThreshold && r.RatingsNumber >= ratingCountThreshold)
                .OrderByDescending(r => r.AverageRating)
                .ThenByDescending(r => r.RatingsNumber)
                .Take(limit)
                .ToList();

            return popularRecipes;
        }

        public async Task<List<Recipe>> GetLatestRecipesAsync(int count)
        {
            var latestRecipes = await _context.Recipes
                .Include(x => x.InformationTime)
                .Include(r => r.Comments)
                    .ThenInclude(c => c.Rating)
                .Select(r => new Recipe
                     {
                         Id = r.Id,
                         Title = r.Title,
                         Description = r.Description,
                         ImageUrl = r.ImageUrl,
                         CreatedAt = r.CreatedAt,
                         UpdatedAt = r.UpdatedAt,
                         Status = r.Status,
                         Slug = r.Slug,
                    InformationTime = r.InformationTime != null ? new InformationTime
                         {
                             Id = r.InformationTime.Id,
                             PrepTime = r.InformationTime.PrepTime,
                             CookTime = r.InformationTime.CookTime,
                             StandTime = r.InformationTime.StandTime,
                             TotalTime = r.InformationTime.TotalTime,
                             Servings = r.InformationTime.Servings
                         } : null,
                         Comments = r.Comments.Select(c => new Comment
                         {
                             Rating = c.Rating
                         }).ToList(),

                     })
                .Where(r => r.CreatedAt != null) 
                .OrderByDescending(r => r.CreatedAt)  
                .Take(100)  
                .OrderBy(r => Guid.NewGuid())  
                .Take(count)  
                .ToListAsync();

            return latestRecipes;
        }
        public async Task<List<Recipe>> GetRecipesByStatusAsync(RecipeStatus status)
        {
            return await _context.Recipes.Where(r => r.Status == status).ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}


