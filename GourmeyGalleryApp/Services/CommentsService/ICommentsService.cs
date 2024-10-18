using GourmeyGalleryApp.Models.DTOs.Comments;
using GourmeyGalleryApp.Models.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GourmeyGalleryApp.Interfaces
{
    public interface ICommentsService
    {
        Task<CommentDto> AddCommentAsync(CommentDto commentDto);
        Task<CommentDto> GetCommentAsync(int id);
        Task<IEnumerable<CommentDto>> GetCommentsForRecipeAsync(int recipeId);
        Task<Comment?> GetCommentByIdAsync(int id);
        Task UpdateCommentAsync(CommentDto commentDto);
        Task DeleteCommentAsync(int id);
        Task MarkAsHelpfulAsync(int commentId, string userId);
    }
}
