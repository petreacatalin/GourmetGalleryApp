using AutoMapper;
using Azure.Storage.Blobs;
using BenchmarkDotNet.Running;
using GourmeyGalleryApp.Models.DTOs.Comments;
using GourmeyGalleryApp.Models.DTOs.Recipe;
using GourmeyGalleryApp.Models.Entities;
using GourmeyGalleryApp.Services;
using GourmeyGalleryApp.Services.RecipeService;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

[ApiController]
[Route("api/recipes")]
public class RecipeController : ControllerBase
{
    private readonly IRecipeService _recipeService;
    private readonly IMapper _mapper;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly BlobStorageService _blobStorageService;

    public RecipeController(
        IRecipeService recipeService,
        IMapper mapper,
        UserManager<ApplicationUser> userManager,
        IConfiguration configuration,
        BlobStorageService blobStorageService
        )
    {
        _recipeService = recipeService;
        _mapper = mapper;
        _userManager = userManager;
        _configuration = configuration;
        _blobStorageService = blobStorageService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<RecipeDto>>> GetRecipes(bool? isAdmin)
    {
        var recipes = await _recipeService.GetAllRecipesAsync(isAdmin);
        var recipesDto = _mapper.Map<IEnumerable<RecipeDto>>(recipes);
        return Ok(recipesDto);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRecipe(int id)
    {
        var recipe = await _recipeService.GetRecipeByIdAsync(id);
        if (recipe == null)
        {
            return NotFound();
        }
        var recipeMapped = _mapper.Map<RecipeDto>(recipe);
        return Ok(recipeMapped);
    }

    [HttpGet("{id}/{slug}")]
    public async Task<IActionResult> GetRecipeByIdAndSlug(int id, string? slug)
    {
        var recipe = await _recipeService.GetRecipeByIdAsync(id);
        if (recipe == null)
        {
            return NotFound();
        }

        if (string.IsNullOrEmpty(slug) || recipe.Slug != slug)
        {
          
            return RedirectToAction(nameof(GetRecipeByIdAndSlug), new { id = recipe.Id, slug = recipe.Slug });
        }

        var recipeMapped = _mapper.Map<RecipeDto>(recipe);
        return Ok(recipeMapped);
    }

    private string GenerateSlug(string name)
    {
        return name.ToLower().Replace(" ", "-").Replace(",", "").Replace("&", "and");
    }

    [HttpGet("ratings/{id}")]
    public async Task<ActionResult<IEnumerable<RatingDto>>> GetRatingsByRecipeId(int id)
    {
        var recipes = await _recipeService.GetRatingsByRecipeId(id);
        if (recipes == null)
        {
            return NotFound();
        }
        var recipesDto = _mapper.Map<IEnumerable<RatingDto>>(recipes);
        return Ok(recipesDto);
    }

    [HttpPost]
    public async Task<IActionResult> CreateRecipe([FromBody] RecipeDto recipeDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var recipe = _mapper.Map<Recipe>(recipeDto);
            var userId = User.FindFirstValue("nameId");

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            recipe.ApplicationUserId = userId;

            if (recipeDto.Instructions != null)
            {
                recipe.Instructions = new Instructions
                {
                    Steps = recipeDto.Instructions.Steps.Select(stepDto => _mapper.Map<Step>(stepDto)).ToList()
                };
            }
            if (recipeDto.IngredientsTotal != null)
            {
                recipe.IngredientsTotal = new IngredientsTotal
                {
                    Ingredients = recipeDto.IngredientsTotal.Ingredients.Select(ingredientDto => _mapper.Map<Ingredient>(ingredientDto)).ToList()
                };
            }
            if (recipeDto.InformationTime != null)
            {
                recipe.InformationTime = _mapper.Map<InformationTime>(recipeDto.InformationTime);
            }
            if (recipeDto.NutritionFacts != null)
            {
                recipe.NutritionFacts = _mapper.Map<NutritionFacts>(recipeDto.NutritionFacts);
            }

            // Handle file upload
            //if (recipeFile != null)
            //{
            //    var blobStorageService = new BlobStorageService(_configuration); // Consider injecting this service
            //    var imageUrl = await blobStorageService.UploadFile(recipeFile);
            //    recipe.ImageUrl = imageUrl; // Save the URL of the uploaded image
            //}

            recipe.CreatedAt = DateTime.UtcNow;
            await _recipeService.AddRecipeAsync(recipe, recipeDto);

            var savedRecipe = await _recipeService.GetRecipeByIdAsync(recipe.Id);
            var recipeDtoResult = _mapper.Map<RecipeDto>(savedRecipe);

            return Ok(recipeDtoResult);
        }
        catch (Exception ex)
        {
            return StatusCode(500,ex.InnerException);
        }
    }
   

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRecipe(int id, [FromBody] RecipeDto recipeDto)
    {
        if (id != recipeDto.Id)
        {
            return BadRequest();
        }

        var recipe = await _recipeService.GetRecipeByIdAsync(id);
        if (recipe == null)
        {
            return NotFound();
        }

        _mapper.Map(recipeDto, recipe);

        try
        {
            var userId = User.FindFirstValue("nameId");
            recipe.ApplicationUserId = userId;
            await _recipeService.UpdateRecipeAsync(recipe);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRecipe(int id)
    {
        try
        {
            await _recipeService.DeleteRecipeAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpGet("popular")]
    public async Task<ActionResult<List<RecipeDto>>> GetPopularRecipes(
     [FromQuery] double ratingThreshold = 3.0,
     [FromQuery] int ratingCountThreshold = 1,
     [FromQuery] int limit = 10)
    {
        var popularRecipes = await _recipeService.GetPopularRecipesAsync(ratingThreshold, ratingCountThreshold, limit);
        return Ok(popularRecipes);
    }

    [HttpGet("latest")]
    public async Task<ActionResult<List<RecipeDto>>> GetLatestRecipesAsync([FromQuery] int limit)
    {
        var latestRecipes = await _recipeService.GetLatestRecipesAsync(limit);

        return Ok(latestRecipes);
    }
}
