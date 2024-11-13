using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;
using static GourmeyGalleryApp.Utils.RecipeEnums;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GourmeyGalleryApp.Models.Entities
{
    public enum RecipeStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
    }

    public class Recipe
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string? Slug { get; set; }
        public RecipeStatus Status { get; set; } = RecipeStatus.Pending;
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

        public int? MealTypeId { get; set; }
        public int? CuisineId { get; set; }
        public int? IngredientId { get; set; }
        public int? OccasionId { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DietaryRestrictions? DietaryRestrictions { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DifficultyLevel? DifficultyLevel { get; set; }
        public int IngredientsTotalId { get; set; }
        public int InstructionsId { get; set; }
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public NutritionFacts? NutritionFacts { get; set; }
        public InformationTime? InformationTime { get; set; }
        public Instructions? Instructions { get; set; }
        public IngredientsTotal? IngredientsTotal { get; set; }
        public ICollection<Comment>? Comments { get; set; }
        public ICollection<RecipeCategory> RecipeCategories { get; set; } = new List<RecipeCategory>();

        [NotMapped]
        public double AverageRating
        {
            get
            {
                if (Comments == null || !Comments.Any())
                    return 0;

                var ratings = Comments
                    .Where(c => c.Rating != null)
                    .Select(c => c.Rating.RatingValue ?? 0);

                if (!ratings.Any())
                    return 0;

                return ratings.Average();
            }
        }
        [NotMapped]
        public double RatingsNumber
        {
            get
            {
                if (Comments == null || !Comments.Any())
                    return 0;

                var ratings = Comments
                    .Where(c => c.Rating != null)
                    .Select(c => c.Rating.RatingValue ?? 0);

                if (!ratings.Any())
                    return 0;

                return ratings.Count();
            }
        }

        [NotMapped]
        public Comment? MostHelpfulPositiveComment { get; set; }

    }

}
