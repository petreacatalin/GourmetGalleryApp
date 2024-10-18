using GourmeyGalleryApp.Interfaces;
using GourmeyGalleryApp.Models.Entities;

namespace GourmeyGalleryApp.Infrastructure
{
    public interface ICommentsRepository : IRepository<Comment>
    {
        Task<IEnumerable<Comment>> GetCommentsForRecipeAsync(int recipeId);
        Task UpdateCommentAsync(Comment comment);
        Task<Comment?> GetCommentByIdAsync(int id);
        Task DeleteCommentAsync(Comment comment);
        Task<IEnumerable<Comment>> GetRepliesAsync(int parentId);
        Task<CommentVote?> GetUserVoteForCommentAsync(int commentId, string userId);
        Task AddOrUpdateVoteAsync(int commentId, string userId);

    }
}
