
using GourmeyGalleryApp.Models.DTOs.ApplicationUser;
using GourmeyGalleryApp.Models.Entities;

namespace GourmeyGalleryApp.Models.DTOs.Comments
{
    public class CommentVoteDto
    {
        public int Id { get; set; }
        public int CommentId { get; set; }
        public string UserId { get; set; }  // To track the user who voted
        public Comment Comment { get; set; }  // Navigation property to the comment
        public ApplicationUserDto User { get; set; }

    }

}
