using GourmetGallery.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        [HttpPost("addMealPlan")]
        public async Task<IActionResult> AddMealPlan([FromBody] MealPlan plan)
        {
            _context.MealPlans.Add(plan);
            await _context.SaveChangesAsync();
            return Ok(plan);
        }

        [HttpGet("getMealPlans/{userId}")]
        public async Task<IActionResult> GetMealPlans(string userId)
        {
            var plans = await _context.MealPlans
                .Where(mp => mp.UserId == userId)
                .Include(mp => mp.Recipe)
                .ToListAsync();

            return Ok(plans);
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
