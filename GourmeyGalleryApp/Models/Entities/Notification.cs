namespace GourmeyGalleryApp.Models.Entities
{
   public enum NotificationType 
   {
        Follow,
        Like,
        Comment,
        Recipe,
    }
    public class Notification
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; } // Foreign key to ApplicationUser
        public ApplicationUser User { get; set; } // Navigation property

        public string Message { get; set; }
        public NotificationType Type { get; set; } // E.g., "FriendRequest", "RecipeLike"
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; } = false;
    }

}
