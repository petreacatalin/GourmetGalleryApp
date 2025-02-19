using GourmetGallery.Infrastructure;
using GourmeyGalleryApp.Models.Entities;
using GourmeyGalleryApp.Services.CategoryService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GourmeyGalleryApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MealPlanController : ControllerBase
    {
        private readonly GourmetGalleryContext _context;

        public MealPlanController(GourmetGalleryContext context)
        {
            _context = context;
        }

        [HttpPost("add-mealplan")]
        public async Task<IActionResult> AddMealPlan([FromBody] MealPlan plan)
        {
            var userId = User.FindFirstValue("nameId");
            plan.UserId = userId;
            _context.MealPlans.Add(plan);
            await _context.SaveChangesAsync();
            return Ok(plan);
        }

        [HttpGet("mealplans")]
        public async Task<IActionResult> GetMealPlans()
        {
            var userId = User.FindFirstValue("nameId"); 
            var plans = await _context.MealPlans
                .Where(mp => mp.UserId == userId)
                .Include(mp => mp.Recipe)
                .ToListAsync();

            return Ok(plans);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMealPlan(int id)
        {

            var mealToBeRemoved = await _context.MealPlans.FindAsync(id);
            if (mealToBeRemoved != null)
            {
                _context.MealPlans.Remove(mealToBeRemoved);
                await _context.SaveChangesAsync();
            }
            return NoContent();
        }
        public class GroceryItem
        {
            public string Name { get; set; }
            public double Quantity { get; set; }
            public string Unit { get; set; }
        }

        [HttpGet("generateGroceryList/{userId}")]
        public async Task<IActionResult> GenerateGroceryList(string userId)
        {
            var mealPlans = await _context.MealPlans
                .Where(mp => mp.UserId == userId)
                .Include(mp => mp.Recipe)
                .ThenInclude(r => r.IngredientsTotal)
                .ToListAsync();

            //var groceryList = mealPlans
            //    .SelectMany(mp => mp.Recipe.IngredientsTotal.Ingredients)
            //    .GroupBy(ing => new { ing.Name, ing. })
            //    .Select(g => new GroceryItem
            //    {
            //        Name = g.Key.Name,
            //        Unit = g.Key.Unit,
            //        Quantity = g.Sum(ing => ing.Quantity)
            //    })
            //    .ToList();

            return Ok(/*groceryList*/);
        }

    }
}
