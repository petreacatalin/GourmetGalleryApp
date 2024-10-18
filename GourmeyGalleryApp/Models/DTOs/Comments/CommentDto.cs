using GourmeyGalleryApp.Models.DTOs.ApplicationUser;
using GourmeyGalleryApp.Models.DTOs.Recipe;
using GourmeyGalleryApp.Models.Entities;
using System.Text.Json.Serialization;

namespace GourmeyGalleryApp.Models.DTOs.Comments
{
    public class CommentDto
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public int RecipeId { get; set; }
        public int? RatingId { get; set; }
        public DateTime Submitted { get; set; } = DateTime.Now;
        public DateTime Updated { get; set; }
        public int HelpfulCount { get; set; } = 0; 
        public int NotHelpfulCount { get; set; } = 0;
        public int TotalHelpful => HelpfulCount - NotHelpfulCount;
        public string? ApplicationUserId { get; set; }
        public RatingDto? Rating { get; set; }
        public ApplicationUserDto? User { get; set; }
        [JsonIgnore]
        public RecipeDto? Recipe { get; set; }
        public bool IsEdited { get; set; }
        public int? ParentCommentId { get; set; }
        [JsonIgnore]
        public CommentDto? ParentComment { get; set; }
        public ICollection<CommentDto>? Replies { get; set; } = new List<CommentDto>();
        public ICollection<CommentVote> Votes { get; set; } = new List<CommentVote>();  

    }
}
