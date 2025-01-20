using GourmeyGalleryApp.Models.Entities;
using GourmeyGalleryApp.Services.NewsletterService;
using Hangfire;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GourmeyGalleryApp.Controllers
{
    public class SubscriptionRequest
    {
        public string Email { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]

    public class NewsletterController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public NewsletterController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost("subscribe")]
        public async Task<IActionResult> UpdateNewsletterSubcription([FromBody] SubscriptionRequest email)
        {
            try
            {
                var userId = User.FindFirstValue("nameId");
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                {
                    return NotFound("User not found");
                }

                if (user.Email == email.Email)
                {
                    user.IsSubscribedToNewsletter = true;

                    await _userManager.UpdateAsync(user);
                    return Ok(new { succes = true, subscribed = user.IsSubscribedToNewsletter });
                }
                else
                {
                    return NotFound("The email is not the same with the actual logged account");
                }

            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

        [HttpPost("unsubscribe")]
        public async Task<IActionResult> UnsubscribeFromNewsletter()
        {
            var userId = User.FindFirstValue("nameId");
            var user = await _userManager.FindByIdAsync(userId);
            try
            {
                user.IsSubscribedToNewsletter = false;
                await _userManager.UpdateAsync(user);
                return Ok(new { succes = true, subscribed = user.IsSubscribedToNewsletter });
            }
            catch (Exception ex)
            {

                throw ex;
            }
           
        }

        [HttpPost("newsletter/send-manually")]
        public IActionResult TriggerNewsletter()
        {
            try
            {
                BackgroundJob.Enqueue<NewsletterService>(service => service.SendNewsletterAsync());

            }
            catch (Exception ex)
            {

                throw ex;
            }

            return Ok(new { success = true, message = "Newsletter job triggered manually." });
        }

    }
}
