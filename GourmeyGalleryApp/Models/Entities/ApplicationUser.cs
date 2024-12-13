using Microsoft.AspNetCore.Identity;

namespace GourmeyGalleryApp.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ProfilePictureUrl { get; set; }
        public bool IsAdmin { get; set; } = false;
        public DateTime JoinedAt { get; set; }
        public string? About { get; set; }
        public ICollection<Friend> FriendsAdded { get; set; }

        // Navigation property representing friends who added the user
        public ICollection<Friend> FriendsAccepted { get; set; }
        public ICollection<Message> MessagesSent { get; set; }
        public ICollection<Message> MessagesReceived { get; set; }
        public ICollection<Comment> Comments { get; set; }
        public ICollection<Rating> Ratings { get; set; }
        public ICollection<MealPlan> MealPlans { get; set; }
        public ICollection<Recipe> Recipes { get; set; }
        public ICollection<UserFavoriteRecipe> UserFavoriteRecipes { get; set; }
        public ICollection<CommentVote> CommentVotes { get; set; } = new List<CommentVote>();  // Votes cast by the user
        public ICollection<UserBadge> UserBadges { get; set; } = new List<UserBadge>();
    }
}
