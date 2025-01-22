using GourmetGallery.Infrastructure;
using GourmeyGalleryApp.Models.Entities;
using GourmeyGalleryApp.Shared.SignalRHub;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace GourmeyGalleryApp.Services.NotificationService
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly GourmetGalleryContext _context;

        public NotificationService(IHubContext<NotificationHub> hubContext, GourmetGalleryContext context)
        {
            _hubContext = hubContext;
            _context = context;
        }

        public async Task SendLikeNotification(string userId, string recipeName)
        {
            string notificationMessage = $"{userId} liked your recipe: {recipeName}";
            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", notificationMessage);
        }

        public async Task SendGlobalNotification(string message)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", message);
        }

        public async Task CreateNotificationAsync(string userId, NotificationType type, string message, int? referenceId = null)
        {
            // Save notification to the database
            var notification = new Notification
            {
                ApplicationUserId = userId,
                Type = type,
                Message = message,
                //ReferenceId = referenceId,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();

            // Notify the user in real-time using SignalR
            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", notification);
        }

        public async Task DismissAllNotificationsAsync(string userId)
        {
            var notifications = _context.Notifications
                .Where(n => n.ApplicationUserId == userId);

            _context.Notifications.RemoveRange(notifications);
            await _context.SaveChangesAsync();
        }
    }

}
