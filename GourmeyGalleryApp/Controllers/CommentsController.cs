using GourmeyGalleryApp.Interfaces;
using GourmeyGalleryApp.Models.DTOs.Comments;
using GourmeyGalleryApp.Models.Entities;
using GourmeyGalleryApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class CommentsController : ControllerBase
{
    private readonly ICommentsService _commentsService;

    public CommentsController(ICommentsService commentsService)
    {
        _commentsService = commentsService;
    }

    [HttpPost]
    public async Task<IActionResult> PostComment([FromBody] CommentDto commentDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        try
        {
            var comment = await _commentsService.AddCommentAsync(commentDto);
            return CreatedAtAction(nameof(GetComment), new { id = comment.Id }, comment);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<CommentDto>> GetComment(int id)
    {
        var comment = await _commentsService.GetCommentAsync(id);

        if (comment == null)
        {
            return NotFound();
        }

        return Ok(comment);
    }

    [HttpGet("recipe/{recipeId}")]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetCommentsForRecipe(int recipeId)
    {
        var comments = await _commentsService.GetCommentsForRecipeAsync(recipeId);
        return Ok(comments);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateComment(int id, [FromBody] CommentDto commentDto)
    {
        try
        {
            commentDto.Id = id; // Ensure the ID is set
            await _commentsService.UpdateCommentAsync(commentDto);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteComment(int id)
    {
        try
        {
            await _commentsService.DeleteCommentAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [Authorize(Roles = "Admin, User")]
    [HttpPost("{id}/helpful")]
    public async Task<IActionResult> MarkAsHelpful(int id)
    {
        var userId = User.FindFirstValue("nameId");
        if (userId == null) return Unauthorized();

        try
        {
            await _commentsService.MarkAsHelpfulAsync(id, userId);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [Authorize(Roles = "Admin, User")]
    [HttpGet("{commentId}/user-vote")]
    public async Task<IActionResult> GetUserVoteForCommentAsync(int commentId)
    {
        var userId = User.FindFirstValue("nameId");

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User not logged in.");
        }

        var existingVote = await _commentsService.GetUserVoteForCommentAsync(commentId, userId);

        if (existingVote != null)
        {
            return Ok(true); 
        }

        return Ok(false); 
    }

}
