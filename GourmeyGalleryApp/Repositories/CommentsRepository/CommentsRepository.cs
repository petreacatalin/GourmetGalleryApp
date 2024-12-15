using GourmetGallery.Infrastructure;
using GourmetGallery.Infrastructure.Repositories;
using GourmeyGalleryApp.Interfaces;
using GourmeyGalleryApp.Models.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GourmeyGalleryApp.Infrastructure
{
    public class CommentsRepository : Repository<Comment>, ICommentsRepository
    {
        private readonly GourmetGalleryContext _context;

        public CommentsRepository(GourmetGalleryContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Comment>> GetCommentsForRecipeAsync(int recipeId)
        {
            return await _context.Comments.AsNoTracking()
       .Where(c => c.RecipeId == recipeId)
       .Include(c => c.Rating) // Include comment rating
       .Include(c => c.User) // Include the user who submitted the comment
           .ThenInclude(u => u.UserBadges) // Include user badges
           .ThenInclude(ub => ub.Badge) // Include badge details
       .Include(c => c.Replies) // Include comment replies
       .OrderByDescending(c => c.Submitted) // Order by submission date (most recent first)
       .ToListAsync();
        }

        public async Task<Comment?> GetCommentByIdAsync(int id)
        {
            return await _context.Comments.FindAsync(id);
        }

        public async Task UpdateCommentAsync(Comment comment)
        {
            _context.Comments.Update(comment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCommentAsync(Comment comment)
        {
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Comment>> GetRepliesAsync(int parentId)
        {
           return await _context.Comments.AsNoTracking().Where(c => c.ParentCommentId == parentId).ToListAsync();
        }
        public async Task<CommentVote?> GetUserVoteForCommentAsync(int commentId, string userId)
        {
            return await _context.CommentVotes.AsNoTracking()   
                .FirstOrDefaultAsync(v => v.CommentId == commentId && v.UserId == userId);
        }

        public async Task AddOrUpdateVoteAsync(int commentId, string userId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
            {
                throw new ArgumentException("Comment not found.");
            }

            // Prevent users from voting on their own comments
            if (comment.ApplicationUserId == userId)
            {
                throw new InvalidOperationException("Users cannot vote on their own comments.");
            }

            // Check if the user has already voted on this comment
            var existingVote = await GetUserVoteForCommentAsync(commentId, userId);

            if (existingVote != null)
            {
                // User is un-voting (removing their vote)
                _context.Remove(existingVote);
                comment.HelpfulCount = Math.Max(0, comment.HelpfulCount - 1); // Ensure count doesn't go below 0
            }
            else
            {
                // User is voting (adding their vote)
                var vote = new CommentVote
                {
                    CommentId = commentId,
                    UserId = userId,
                };

                _context.CommentVotes.Add(vote);
                comment.HelpfulCount++; // Increase helpful count
            }

            await _context.SaveChangesAsync();
        }



    }
}
