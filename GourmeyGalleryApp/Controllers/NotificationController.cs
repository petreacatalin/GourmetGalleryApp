using GourmetGallery.Infrastructure;
using GourmeyGalleryApp.Models.Entities;
using GourmeyGalleryApp.Services.NotificationService;
using GourmeyGalleryApp.Shared.SignalRHub;
using GourmeyGalleryApp.Utils.FactoryPolicies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GourmeyGalleryApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]       

    public class NotificationsController : ControllerBase
    {
        private readonly GourmetGalleryContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly INotificationService _notificationService;
        private readonly IAsyncPolicyFactory _policyFactory;

        public NotificationsController(GourmetGalleryContext context, IHubContext<NotificationHub> hubContext, INotificationService notificationService, IAsyncPolicyFactory policyFactory)
        {
            _context = context;
            _hubContext = hubContext;
            _notificationService = notificationService;
            _policyFactory = policyFactory;
        }

        // Existing method to get notifications
        [HttpGet]
        public async Task<IActionResult> GetUserNotifications()
        {
            var userId = User.FindFirstValue("nameId");

            var notifications = await _context.Notifications
                .Where(n => n.ApplicationUserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
            return Ok(notifications);
        }

        // Method to mark a notification as read
        [HttpPost("mark-read/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                var resendPolicy = _policyFactory.GetPolicy("NotificationMarkAsReadPolicy");

                var notification = await _context.Notifications.FindAsync(id);

                if (notification == null)
                {
                    return NotFound();
                }

                await resendPolicy.ExecuteAsync(async () =>
                {
                    notification.IsRead = true;
                    await _context.SaveChangesAsync();

                });
                    return Ok(notification);    

            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        // Method to create a new notification and send it to the user in real-time
        [HttpPost("create")]
        public async Task<IActionResult> CreateNotification([FromBody] Notification newNotification)
        {
            // Save the notification to the database
            _context.Notifications.Add(newNotification);
            await _context.SaveChangesAsync();

            // Trigger the real-time notification to the user
            await _hubContext.Clients.User(newNotification.ApplicationUserId).SendAsync("ReceiveNotification", "You have a new notification!");

            return Ok(newNotification);
        }

        // Method to create a new notification and send it to the user in real-time
        [Authorize(Roles = "Admin, User")]
        [HttpDelete("clear-all-notifications")]
        public async Task<IActionResult> DismissAllNotifications()
        {
            var userId = User.FindFirstValue("nameId");

            // Logic to mark all notifications as dismissed for the user
            await _notificationService.DismissAllNotificationsAsync(userId);

            return Ok(new { success = true, message = "All notifications dismissed." });
        }
    }
}


