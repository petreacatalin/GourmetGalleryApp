using GourmeyGalleryApp.Models.Entities;
using GourmeyGalleryApp.Repositories.BadgeRepository;

namespace GourmeyGalleryApp.Services.BadgeService
{
    public class BadgeService : IBadgeService
    {
        private readonly IBadgeRepository _badgeRepository;

        public BadgeService(IBadgeRepository badgeRepository)
        {
            _badgeRepository = badgeRepository;
        }

        public async Task<IEnumerable<Badge>> GetBadgesAsync()
        {
            return await _badgeRepository.GetBadgesAsync();
        }

        public async Task<IEnumerable<Badge>> GetUserBadgesAsync(string userId)
        {
            return await _badgeRepository.GetUserBadgesAsync(userId);
        }

        public async Task ProcessUserBadgesAsync(string userId) 
        { 

            await _badgeRepository.ProcessUserBadgesAsync(userId);
        }

        public async Task<Badge> GetBadgeByIdAsync(int id)
        {
            return await _badgeRepository.GetBadgeByIdAsync(id);
        }

        public async Task<Badge> CreateBadgeAsync(Badge badge)
        {
            await _badgeRepository.AddBadgeAsync(badge);
            return badge;
        }

        public async Task UpdateBadgeAsync(int id, Badge badge)
        {
            var existingBadge = await _badgeRepository.GetBadgeByIdAsync(id);
            if (existingBadge == null)
            {
                throw new KeyNotFoundException("Badge not found.");
            }

            existingBadge.Name = badge.Name;
            existingBadge.Description = badge.Description;
            existingBadge.IconUrl = badge.IconUrl;
            existingBadge.Points = badge.Points;
            existingBadge.Condition = badge.Condition;
            existingBadge.IsActive = badge.IsActive;

            await _badgeRepository.UpdateBadgeAsync(existingBadge);
        }

        public async Task DeleteBadgeAsync(int id)
        {
            await _badgeRepository.DeleteBadgeAsync(id);
        }
    }

}