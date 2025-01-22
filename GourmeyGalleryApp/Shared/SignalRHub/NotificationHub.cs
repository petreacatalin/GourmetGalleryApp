using GourmeyGalleryApp.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace GourmeyGalleryApp.Shared.SignalRHub
{
    public class NotificationHub : Hub
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationHub(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

    
        // Method to send notification to a specific user
        public async Task SendNotificationToUser(string userId, Notification notification)
        {
            await Clients.User(userId).SendAsync("ReceiveNotification", notification);
        }

        public async Task SendNotificationToAll(string message)
        {
            await Clients.All.SendAsync("ReceiveNotification", message);
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.Identity?.Name;
            //var userClaims = Context.User?.Claims;

            //if (userClaims != null)
            //{
            //    foreach (var claim in userClaims)
            //    {
            //        Console.WriteLine($"{claim.Type}: {claim.Value}");
            //    }
            //}

            if (userId == null)
            {
                throw new HubException("User not authenticated.");
            }

            await base.OnConnectedAsync();
        }
    }
}
