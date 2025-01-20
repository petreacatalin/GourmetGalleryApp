
using GourmeyGalleryApp.Models.Entities;
using GourmeyGalleryApp.Services.EmailService;
using GourmeyGalleryApp.Services.RecipeService;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Text;

namespace GourmeyGalleryApp.Services.NewsletterService
{
    public class NewsletterService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<NewsletterService> _logger;
        private readonly IEmailService _emailService;
        private readonly IRecipeService _recipeService;
        public NewsletterService(UserManager<ApplicationUser> userManager, 
            ILogger<NewsletterService> logger,
            IEmailService emailService,
            IRecipeService recipeService)
        {
            _userManager = userManager;
            _logger = logger;
            _emailService = emailService;
            _recipeService = recipeService;
        }

        public async Task SendNewsletterAsync()
        {
            var subscribedUsers = await _userManager.Users
                .Where(nws => nws.IsSubscribedToNewsletter)
                .ToListAsync();

            if (!subscribedUsers.Any())
            {
                _logger.LogInformation("No users are subscribed to newsletter");
                return;
            }
            var popularRecipes = await GetPopularRecipes();

            foreach (var user in subscribedUsers) 
            {
                try
                {
                    var subject = "Explore the Best Recipes This Week! - Gourmet Gallery Newsletter";
                    var content = GenerateNewsletterContent(user, popularRecipes);
                    await _emailService.SendEmailAsync(user.Email, subject, await GenerateNewsletterContent(user, popularRecipes));
                    _logger.LogInformation($"Newsletter sent to {user.Email}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Failed to send newsletter to {user.Email}: {ex.Message}");
                }
            }

        }

        public async Task<List<Recipe>> GetPopularRecipes()
        {
            double ratingThreshold = 3.0;
            int ratingCountThreshold = 1;
            int limit = 10;
            var popularRecipes = await _recipeService.GetPopularRecipesAsync(ratingThreshold, ratingCountThreshold, limit);
            return popularRecipes;

        }
        private async Task<string> GenerateNewsletterContent(ApplicationUser user, List<Recipe>? recipes)
        {
            string baseUrl = "https://gourmetgallery.azurewebsites.net"; // Your actual base URL
            var sb = new StringBuilder();

            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<style>");
            sb.AppendLine("  body { font-family: Arial, sans-serif; margin: 0; padding: 0; color: #333; }");
            sb.AppendLine("  .email-container { max-width: 600px; margin: 20px auto; padding: 20px; border: 1px solid #ddd; border-radius: 8px; background-color: #f9f9f9; }");
            sb.AppendLine("  h1 { color: #5a9; }");
            sb.AppendLine("  p { line-height: 1.6; }");

            // Make sure the grid has 3 columns per row
            sb.AppendLine("  .grid-container { display: grid;  grid-template-columns: repeat(2, 1fr); gap: 10px; margin-top: 20px; }");
            sb.AppendLine("  .grid-item { border: 1px solid #ddd; border-radius: 8px; padding: 10px; text-align: center; background-color: #fff; overflow: hidden; position: relative; }");

            // Adjust image size and ensure it fits within the grid item
            sb.AppendLine("  .grid-item img { width: 100%; height: 150px; object-fit: cover; border-radius: 4px; }"); // Limiting image height and setting object-fit
            sb.AppendLine("  .grid-item a { display: block; position: relative; text-decoration: none; }"); // Make the entire grid item clickable
            sb.AppendLine("  .grid-item a:hover { text-decoration: none; }");

            sb.AppendLine("  .grid-item a .recipe-title { margin-top: 10px; text-decoration: none; color: #007bff; font-weight: bold; font-size: 14px; }"); // Recipe title text styling

            sb.AppendLine("  .footer { margin-top: 20px; font-size: 0.9em; color: #555; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");
            sb.AppendLine("  <div class='email-container'>");
            sb.AppendLine($"    <h1>Hello {user.FirstName}!</h1>");
            sb.AppendLine("    <p>We hope this email finds you well. Here are some delicious recipes we’ve picked for you this week:</p>");
            sb.AppendLine("    <div class='grid-container'>");

            // Add grid items dynamically for each recipe
            foreach (var recipe in recipes)
            {
                string recipeUrl = $"{baseUrl}/recipes/{recipe.Id}/{recipe.Slug}";
                sb.AppendLine("      <div class='grid-item'>");
                sb.AppendLine($"        <a href='{recipeUrl}'>"); // Wrap the entire grid item in an anchor tag
                sb.AppendLine($"          <img src='{recipe.ImageUrl}' alt='{recipe.Title}'>");
                sb.AppendLine($"          <span class='recipe-title'>{recipe.Title}</span>"); // Recipe title within the anchor tag
                sb.AppendLine("        </a>");
                sb.AppendLine("      </div>");
            }

            sb.AppendLine("    </div>"); // Close grid-container
            sb.AppendLine("    <p>Happy cooking!</p>");
            sb.AppendLine("    <p class='footer'>");
            sb.AppendLine("      If you no longer wish to receive these emails, you can <a href='#'>unsubscribe here</a>.");
            sb.AppendLine("    </p>");
            sb.AppendLine("    <p class='footer'>The Gourmet Gallery Team</p>");
            sb.AppendLine("  </div>");
            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            return sb.ToString();
        }






    }
}
