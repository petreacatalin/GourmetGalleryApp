using GourmeyGalleryApp.Models.Entities;

namespace GourmeyGalleryApp.Services.BadgeService
{
    public interface IBadgeService
    {
        Task<IEnumerable<Badge>> GetBadgesAsync();
        Task<IEnumerable<Badge>> GetUserBadgesAsync(string userId);
        Task ProcessUserBadgesAsync(string userId);
        //CRUD        
        Task<Badge> GetBadgeByIdAsync(int id);
        Task<Badge> CreateBadgeAsync(Badge badge);
        Task UpdateBadgeAsync(int id, Badge badge);
        Task DeleteBadgeAsync(int id);
    }

}