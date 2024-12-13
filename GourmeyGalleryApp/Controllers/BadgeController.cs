using GourmeyGalleryApp.Models.Entities;
using GourmeyGalleryApp.Services.BadgeService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GourmeyGalleryApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BadgesController : ControllerBase
    {
        private readonly IBadgeService _badgeService;

        public BadgesController(IBadgeService badgeService)
        {
            _badgeService = badgeService;
        }

        // GET: api/Badges
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Badge>>> GetBadges()
        {
            var badges = await _badgeService.GetBadgesAsync();
            return Ok(badges);

        }

        // POST: api/Badges
        [HttpPost("create-badge")]
        public async Task<ActionResult<Badge>> CreateBadge([FromBody] Badge badge)
        {
            if (badge == null)
                return BadRequest();

            var createdBadge = await _badgeService.CreateBadgeAsync(badge);
            return CreatedAtAction(nameof(GetBadges), new { id = createdBadge.Id }, createdBadge);
        }

        // PUT: api/Badges/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBadge(int id, [FromBody] Badge badge)
        {
            try
            {
                await _badgeService.UpdateBadgeAsync(id, badge);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // DELETE: api/Badges/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBadge(int id)
        {
            try
            {
                await _badgeService.DeleteBadgeAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
     

        [HttpGet("user-badges")]
        public async Task<IActionResult> GetUserBadges()
        {
            var userId = User.FindFirstValue("nameId");
            if(userId == null)
            {
                return BadRequest("Not logged in");
            }
            var userBadges = await _badgeService.GetUserBadgesAsync(userId);
            return Ok(userBadges);
        }

        [HttpPost("process-badges")]
        public async Task<IActionResult> ProcessUserBadgesAsync()
        {
            var userId = User.FindFirstValue("nameId");
            if (userId == null)
            {
                return BadRequest("Not logged in");
            }
            await _badgeService.ProcessUserBadgesAsync(userId);
            return Ok();
        }
    }

}
