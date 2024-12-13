using GourmeyGalleryApp.Models.Entities;

namespace GourmeyGalleryApp.Models.DTOs.ApplicationUser
{
    public class BadgeDto
    {
        public int Id { get; set; }
        public string Name { get; set; }  // e.g., "First Recipe Posted"
        public string Description { get; set; }  // Badge description
        public string IconUrl { get; set; }  // URL for the badge icon
        public int Points { get; set; }  // Points awarded for the badge
        public BadgeCondition Condition { get; set; }  // Condition to trigger badge (e.g., "First Recipe", "5 Recipes")
        public bool IsActive { get; set; }
    }

}
