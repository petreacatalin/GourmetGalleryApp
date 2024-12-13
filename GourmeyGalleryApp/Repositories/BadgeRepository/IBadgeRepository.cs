using GourmeyGalleryApp.Models.Entities;

namespace GourmeyGalleryApp.Repositories.BadgeRepository
{
    public interface IBadgeRepository
    {
        Task<IEnumerable<Badge>> GetBadgesAsync();
        Task<IEnumerable<Badge>> GetUserBadgesAsync(string userId);
        Task ProcessUserBadgesAsync(string userId);
        Task<Badge> GetBadgeByIdAsync(int id);
        Task AddBadgeAsync(Badge badge);
        Task UpdateBadgeAsync(Badge badge);
        Task DeleteBadgeAsync(int id);
    }

}
