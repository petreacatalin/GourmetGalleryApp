using GourmeyGalleryApp.Models.Entities;

public class MealPlan
    {
    public int Id { get; set; }
    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }
    public DateTime Date { get; set; }
    public string MealType { get; set; } // e.g., Breakfast, Lunch, Dinner
    public int RecipeId { get; set; }
    public Recipe? Recipe { get; set; }
}
