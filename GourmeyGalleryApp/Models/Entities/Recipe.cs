﻿using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;
using static GourmeyGalleryApp.Utils.RecipeEnums;

namespace GourmeyGalleryApp.Models.Entities
{

    public class Recipe
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string? ImageUrl { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MealType? MealType { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Cuisine? Cuisine { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DietaryRestrictions? DietaryRestrictions { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public CookingMethod? CookingMethod { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public MainIngredient? MainIngredient { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public Occasion? Occasion { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public DifficultyLevel? DifficultyLevel { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public OtherCategories? OtherCategories { get; set; }
        public int IngredientsTotalId { get; set; }
        public int InstructionsId { get; set; }
        public string ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public NutritionFacts? NutritionFacts { get; set; }
        public InformationTime? InformationTime { get; set; }
        public Instructions Instructions { get; set; }
        public IngredientsTotal IngredientsTotal { get; set; }
        public ICollection<Comment> Comments { get; set; }
    }

}
