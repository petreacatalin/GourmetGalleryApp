using GourmeyGalleryApp.Models.Entities;

namespace GourmeyGalleryApp.Models.DTOs.ApplicationUser
{
    public class UserBadgeDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public bool IsActive { get; set; }

        public int BadgeId { get; set; }
       // public BadgeDto Badge { get; set; }
        public DateTime EarnedDate { get; set; }
    }
}
