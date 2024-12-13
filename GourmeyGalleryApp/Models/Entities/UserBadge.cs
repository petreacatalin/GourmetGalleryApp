namespace GourmeyGalleryApp.Models.Entities
{
    public class UserBadge
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public int BadgeId { get; set; }
        public Badge Badge { get; set; }

        public DateTime EarnedDate { get; set; }
    }


}
