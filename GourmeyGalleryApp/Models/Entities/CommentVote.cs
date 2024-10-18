using GourmeyGalleryApp.Models.DTOs.ApplicationUser;

namespace GourmeyGalleryApp.Models.Entities
{
    public class CommentVote
    {
        public int Id { get; set; }
        public int CommentId { get; set; }
        public string UserId { get; set; }  // To track the user who voted
        public Comment Comment { get; set; }  // Navigation property to the comment
        public ApplicationUser User { get; set; }
    }

}
