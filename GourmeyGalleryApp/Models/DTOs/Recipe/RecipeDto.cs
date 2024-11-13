using GourmeyGalleryApp.Models.DTOs.ApplicationUser;
using GourmeyGalleryApp.Models.DTOs.Comments;
using GourmeyGalleryApp.Models.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using static GourmeyGalleryApp.Utils.RecipeEnums;

namespace GourmeyGalleryApp.Models.DTOs.Recipe
{
    public class RecipeDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? ApplicationUserId { get; set; }
        public int IngredientsTotalId { get; set; }
        public int InstructionsId { get; set; }
        public string? Tags { get; set; }
        public string? ImageUrl { get; set; }
        public string? Slug { get; set; }
        public RecipeStatus Status { get; set; }
        public DateTime? CreatedAt { get; set; } 
        public DateTime? UpdatedAt { get; set; }

        public int? MealTypeId { get; set; }
        public int? CuisineId { get; set; }
        public int? IngredientId { get; set; }
        public int? OccasionId { get; set; }
        public List<int> SelectedSubcategories { get; set; } = new List<int>(); // Selected subcategory IDs
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DietaryRestrictions? DietaryRestrictions { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DifficultyLevel? DifficultyLevel { get; set; }
        public ICollection<CommentDto> Comments { get; set; } = new List<CommentDto>();
        // public ICollection<ReviewDto> Reviews { get; set; } = new List<ReviewDto>();
        public IngredientsTotalDto? IngredientsTotal { get; set; } // Updated DTO
        public InstructionsDto? Instructions { get; set; } // Updated DTO
        public NutritionFactsDto? NutritionFacts { get; set; }
        public InformationTimeDto? InformationTime { get; set; }
        public ApplicationUserDto? ApplicationUser { get; set; }

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

       
        public CommentDto? MostHelpfulPositiveComment { get; set; }
    }
}


