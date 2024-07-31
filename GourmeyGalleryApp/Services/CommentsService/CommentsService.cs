﻿using AutoMapper;
using GourmeyGalleryApp.Infrastructure;
using GourmeyGalleryApp.Interfaces;
using GourmeyGalleryApp.Models.DTOs;
using GourmeyGalleryApp.Models.DTOs.ApplicationUser;
using GourmeyGalleryApp.Models.DTOs.Comments;
using GourmeyGalleryApp.Models.Entities;
using Microsoft.EntityFrameworkCore;
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
                Timestamp = commentDto.Timestamp,
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
                    .Include(c => c.User)
                    .Include(c => c.Replies).ThenInclude(r => r.User) // Include replies and their users
                    .Include(c => c.Replies).ThenInclude(r => r.Replies) // Optionally include nested replies
            );

            if (comment == null) return null;

            var commentDto = new CommentDto
            {
                Id = comment.Id,
                Content = comment.Content,
                RecipeId = comment.RecipeId,
                ApplicationUserId = comment.ApplicationUserId,
                Timestamp = comment.Timestamp,
                RatingId = comment.RatingId,
                Rating = comment.Rating != null ? new RatingDto
                {
                    RatingValue = comment.Rating.RatingValue,
                    UserId = comment.Rating.UserId,
                    RecipeId = comment.Rating.RecipeId,
                } : null,
                User = new ApplicationUserDto
                {
                    Id = comment.User.Id,
                    FirstName = comment.User.FirstName,
                    LastName = comment.User.LastName,
                    ProfilePictureUrl = comment.User.ProfilePictureUrl
                },
                ParentCommentId = comment.ParentCommentId,
                ParentComment = comment.ParentComment != null ? new CommentDto
                {
                    Id = comment.ParentComment.Id,
                    Content = comment.ParentComment.Content,
                    RecipeId = comment.ParentComment.RecipeId,
                    ApplicationUserId = comment.ParentComment.ApplicationUserId,
                    Timestamp = comment.ParentComment.Timestamp,
                    RatingId = comment.ParentComment.RatingId,
                    User = new ApplicationUserDto
                    {
                        Id = comment.ParentComment.User.Id,
                        FirstName = comment.ParentComment.User.FirstName,
                        LastName = comment.ParentComment.User.LastName,
                        ProfilePictureUrl = comment.ParentComment.User.ProfilePictureUrl
                    }
                } : null,
                Replies = comment.Replies != null
                    ? _mapper.Map<List<CommentDto>>(comment.Replies.Where(r => r.IsReply && r.ParentCommentId == comment.Id).ToList())
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
                Timestamp = c.Timestamp,
                RatingId = c.RatingId,
                Rating = c.Rating != null ? new RatingDto
                {
                    RatingValue = c.Rating.RatingValue,
                    UserId = c.Rating.UserId,
                    RecipeId = c.Rating.RecipeId,
                } : null,
                User = new ApplicationUserDto
                {
                    Id = c.User.Id,
                    FirstName = c.User.FirstName,
                    LastName = c.User.LastName,
                    ProfilePictureUrl = c.User.ProfilePictureUrl
                },
                ParentCommentId = c.ParentCommentId,
                ParentComment = c.ParentComment != null ? new CommentDto
                {
                    Id = c.ParentComment.Id,
                    Content = c.ParentComment.Content,
                    RecipeId = c.ParentComment.RecipeId,
                    ApplicationUserId = c.ParentComment.ApplicationUserId,
                    Timestamp = c.ParentComment.Timestamp,
                    RatingId = c.ParentComment.RatingId,
                    User = new ApplicationUserDto
                    {
                        Id = c.ParentComment.User.Id,
                        FirstName = c.ParentComment.User.FirstName,
                        LastName = c.ParentComment.User.LastName,
                        ProfilePictureUrl = c.ParentComment.User.ProfilePictureUrl
                    }
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
            comment.Timestamp = DateTime.UtcNow;

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
    }
}
