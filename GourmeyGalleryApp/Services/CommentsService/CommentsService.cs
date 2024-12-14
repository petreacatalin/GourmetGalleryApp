using AutoMapper;
using GourmeyGalleryApp.Infrastructure;
using GourmeyGalleryApp.Interfaces;
using GourmeyGalleryApp.Models.DTOs;
using GourmeyGalleryApp.Models.DTOs.ApplicationUser;
using GourmeyGalleryApp.Models.DTOs.Comments;
using GourmeyGalleryApp.Models.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GourmeyGalleryApp.Services
{
    public class CommentsService : ICommentsService
    {
        private readonly ICommentsRepository _commentsRepository;
        private readonly IRepository<ApplicationUser> _userRepository;
        private readonly IMapper _mapper;

        public CommentsService(ICommentsRepository commentsRepository, IRepository<ApplicationUser> userRepository, IMapper mapper)
        {
            _commentsRepository = commentsRepository;
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<CommentDto> AddCommentAsync(CommentDto commentDto)
        {
            var user = await _userRepository.GetByIdAsync(commentDto.ApplicationUserId);
            if (user == null)
            {
                throw new ArgumentException("User not found.");
            }

            var listReplies = commentDto.Replies != null
                ? _mapper.Map<ICollection<Comment>>(commentDto.Replies)
                : new List<Comment>();

            var comment = new Comment
            {
                Content = commentDto.Content,
                RecipeId = commentDto.RecipeId,
                ApplicationUserId = commentDto.ApplicationUserId,
                Submitted = DateTime.Now,
                Updated = commentDto.Updated,
                HelpfulCount = commentDto.HelpfulCount,
                NotHelpfulCount = commentDto.NotHelpfulCount,
                IsEdited = commentDto.IsEdited,
                User = user,
                Rating = commentDto.Rating != null ? new Rating
                {
                    RatingValue = commentDto.Rating.RatingValue,
                    UserId = commentDto.ApplicationUserId,
                    RecipeId = commentDto.RecipeId,
                } : null,
                ParentCommentId = commentDto.ParentCommentId,
                Replies = listReplies
            };

            await _commentsRepository.AddAsync(comment);
            await _commentsRepository.SaveChangesAsync();

            return await GetCommentAsync(comment.Id); // Return the full comment with replies
        }


        public async Task<CommentDto> GetCommentAsync(int id)
        {
            var comment = await _commentsRepository.GetFirstOrDefaultAsync(
                c => c.Id == id,
                include: query => query
                    .Include(c => c.User).ThenInclude(u => u.UserBadges).ThenInclude(b=>b.Badge) // Include UserBadges for the user
                    .Include(c => c.Replies).ThenInclude(r => r.User)
                    .Include(c => c.Replies).ThenInclude(r => r.Replies)
            );

            if (comment == null) return null;

            var commentDto = new CommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                RecipeId = comment.RecipeId,
                ApplicationUserId = comment.ApplicationUserId,
                Submitted = comment.Submitted,
                Updated = comment.Updated,
                HelpfulCount = comment.HelpfulCount,
                NotHelpfulCount = comment.NotHelpfulCount,
                RatingId = comment.RatingId,
                IsEdited = comment.IsEdited,
                Rating = comment.Rating != null ? new RatingDto
                {
                    RatingValue = comment.Rating.RatingValue,
                    UserId = comment.Rating.UserId,
                    RecipeId = comment.Rating.RecipeId,
                } : null,
                User = comment.User != null ? new ApplicationUserDto
                {
                    Id = comment.User.Id,
                    FirstName = comment.User.FirstName,
                    LastName = comment.User.LastName,
                    ProfilePictureUrl = comment.User.ProfilePictureUrl,
                    Badges = comment.User.UserBadges != null
              ? comment.User.UserBadges.Select(ub => new BadgeDto
              {
                  IconUrl = ub.Badge.IconUrl,
                  Name = ub.Badge.Name,
                  Description = ub.Badge.Description,
                  Points = ub.Badge.Points,
                  //UserId = ub.UserId,
                  //EarnedDate = ub.EarnedDate
              }).ToList()
              : new List<BadgeDto>()
                } : null,
                ParentCommentId = comment.ParentCommentId,
                ParentComment = comment.ParentComment != null ? new CommentDto
                {
                    Id = comment.ParentComment.Id,
                    Content = comment.ParentComment.Content,
                    RecipeId = comment.ParentComment.RecipeId,
                    ApplicationUserId = comment.ParentComment.ApplicationUserId,
                    Submitted = comment.ParentComment.Submitted,
                    Updated = comment.ParentComment.Updated,
                    RatingId = comment.ParentComment.RatingId,
                    User = new ApplicationUserDto
                    {
                        Id = comment.ParentComment.User.Id,
                        FirstName = comment.ParentComment.User.FirstName,
                        LastName = comment.ParentComment.User.LastName,
                        ProfilePictureUrl = comment.ParentComment.User.ProfilePictureUrl,
                        Badges = comment.User.UserBadges != null
              ? comment.User.UserBadges.Select(ub => new BadgeDto
              {
                  IconUrl = ub.Badge.IconUrl,
                  Name = ub.Badge.Name,
                  Description = ub.Badge.Description,
                  Points = ub.Badge.Points
                  //UserId = ub.UserId,
                  //EarnedDate = ub.EarnedDate
              }).ToList()
              : new List<BadgeDto>()
                    }
                } : null,
                Replies = comment.Replies != null
                    ? _mapper.Map<List<CommentDto>>(comment.Replies.Where(r => r.ParentCommentId == comment.Id).ToList())
                    : new List<CommentDto>(),
            };

            return commentDto;
        }

        public async Task<IEnumerable<CommentDto>> GetCommentsForRecipeAsync(int recipeId)
        {
            // Fetch all comments for the recipe, including replies
            var comments = await _commentsRepository.GetCommentsForRecipeAsync(recipeId);

            // Create a dictionary to hold the CommentDto objects
            var commentDtos = comments.Select(c => new CommentDto
            {
                Id = c.Id,
                Content = c.Content,
                RecipeId = c.RecipeId,
                ApplicationUserId = c.ApplicationUserId,
                Submitted = c.Submitted,
                Updated = c.Updated,
                HelpfulCount = c.HelpfulCount,
                NotHelpfulCount = c.NotHelpfulCount,
                RatingId = c.RatingId,
                IsEdited = c.IsEdited,
                Rating = c.Rating != null ? new RatingDto
                {
                    RatingValue = c.Rating.RatingValue,
                    UserId = c.Rating.UserId,
                    RecipeId = c.Rating.RecipeId,
                } : null,
                User = c.User != null ? new ApplicationUserDto
                {
                    Id = c.User.Id,
                    FirstName = c.User.FirstName,
                    LastName = c.User.LastName,
                    ProfilePictureUrl = c.User.ProfilePictureUrl,
                    Badges = c.User.UserBadges != null
              ? c.User.UserBadges.Select(ub => new BadgeDto
              {
                  IconUrl = ub.Badge.IconUrl,
                  Name = ub.Badge.Name,
                  Description = ub.Badge.Description,
                  Points = ub.Badge.Points
                  //UserId = ub.UserId,
                  //EarnedDate = ub.EarnedDate
              }).ToList()
              : new List<BadgeDto>()
                } : null,
                ParentCommentId = c.ParentCommentId,
                ParentComment = c.ParentComment != null ? new CommentDto
                {
                    Id = c.ParentComment.Id,
                    Content = c.ParentComment.Content,
                    RecipeId = c.ParentComment.RecipeId,
                    ApplicationUserId = c.ParentComment.ApplicationUserId,
                    Updated = c.ParentComment.Updated,
                    Submitted = c.ParentComment.Submitted,
                    RatingId = c.ParentComment.RatingId,
                    User = c.ParentComment.User != null ? new ApplicationUserDto
                    {
                        Id = c.ParentComment.User.Id,
                        FirstName = c.ParentComment.User.FirstName,
                        LastName = c.ParentComment.User.LastName,
                        ProfilePictureUrl = c.ParentComment.User.ProfilePictureUrl,
                        Badges = c.User.UserBadges != null
              ? c.User.UserBadges.Select(ub => new BadgeDto
              {
                  IconUrl = ub.Badge.IconUrl,
                  Name = ub.Badge.Name,
                  Points = ub.Badge.Points,
                  Description = ub.Badge.Description
                  //UserId = ub.UserId,
                  //EarnedDate = ub.EarnedDate
              }).ToList()
              : new List<BadgeDto>()
                    } : null,
                } : null,
                Replies = new List<CommentDto>() // Initialize with an empty list
            }).ToDictionary(c => c.Id);
            // Organize comments by adding replies to their parent comments
            foreach (var comment in commentDtos.Values)
            {
                if (comment.ParentCommentId.HasValue && commentDtos.TryGetValue(comment.ParentCommentId.Value, out var parentComment))
                {
                    parentComment.Replies.Add(comment);
                }
            }

            // Filter and return only top-level comments
            return commentDtos.Values.Where(c => !c.ParentCommentId.HasValue);
        }




        public async Task<Comment?> GetCommentByIdAsync(int id)
        {
            return await _commentsRepository.GetCommentByIdAsync(id);
        }


        public async Task UpdateCommentAsync(CommentDto commentDto)
        {
            var comment = await _commentsRepository.GetCommentByIdAsync(commentDto.Id);
            if (comment == null || comment.ApplicationUserId != commentDto.ApplicationUserId)
            {
                throw new KeyNotFoundException("Comment not found or user is not authorized.");
            }

            comment.Content = commentDto.Content;
            comment.IsEdited = true;
            comment.Updated = DateTime.Now;

            await _commentsRepository.UpdateCommentAsync(comment);
        }

        public async Task DeleteCommentAsync(int id)
        {
            var comment = await _commentsRepository.GetCommentByIdAsync(id);
            if (comment == null)
            {
                throw new KeyNotFoundException("Comment not found.");
            }

            // Recursively delete replies
            await DeleteCommentWithRepliesAsync(comment);
        }

        private async Task DeleteCommentWithRepliesAsync(Comment comment)
        {
            // First, delete all replies
            var replies = await _commentsRepository.GetRepliesAsync(comment.Id);
            foreach (var reply in replies)
            {
                await DeleteCommentWithRepliesAsync(reply);
            }

            // Then, delete the comment itself
            await _commentsRepository.DeleteCommentAsync(comment);
        }

        public async Task MarkAsHelpfulAsync(int commentId, string userId)
        {
            // Validate and add vote
            await _commentsRepository.AddOrUpdateVoteAsync(commentId, userId);
        }

        public async Task<CommentVote?> GetUserVoteForCommentAsync(int commentId, string userId)
        {
            return await _commentsRepository.GetUserVoteForCommentAsync(commentId, userId);
        }

    }
}
