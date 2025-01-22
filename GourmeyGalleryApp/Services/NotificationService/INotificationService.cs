using GourmeyGalleryApp.Models.Entities;

namespace GourmeyGalleryApp.Services.NotificationService
{
    public interface INotificationService
    {
        Task SendLikeNotification(string userId, string recipeName);
        Task SendGlobalNotification(string message);
        Task CreateNotificationAsync(string userId, NotificationType type, string message, int? referenceId = null);
        Task DismissAllNotificationsAsync(string userId);
    }

}
