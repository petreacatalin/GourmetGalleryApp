using GourmetGallery.Infrastructure;
using GourmeyGalleryApp.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace GourmeyGalleryApp.Repositories.BadgeRepository
{
    public class BadgeRepository : IBadgeRepository
    {
        private readonly GourmetGalleryContext _context;

        public BadgeRepository(GourmetGalleryContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Badge>> GetBadgesAsync()
        {
            var badgesQuery = _context.Badges.AsQueryable();

            // Pagination logic
            var badges = await badgesQuery.ToListAsync();

            return badges;
        }

        private async Task<bool> IsConditionMetAsync(string userId, Badge badge)
        {
            switch (badge.Condition)
            {
                case BadgeCondition.FirstRecipePosted:
                    return await _context.Recipes.AnyAsync(r => r.ApplicationUserId == userId);
                case BadgeCondition.RecipeEnthusiast:
                    return await _context.Recipes.CountAsync(r => r.ApplicationUserId == userId) >= 5;
                case BadgeCondition.RecipeMaster:
                    return await _context.Recipes.CountAsync(r => r.ApplicationUserId == userId) >= 20;
                case BadgeCondition.RecipeExplorer:
                    return await _context.Recipes
                        .Where(r => r.ApplicationUserId != userId)
                        .CountAsync() >= 100;
                case BadgeCondition.EarlyAdopter:
                    var user = await _context.Users.FindAsync(userId);
                    return user?.JoinedAt <= DateTime.UtcNow.AddDays(-30);
                case BadgeCondition.OneYearAnniversary:
                    return await _context.Users
                        .Where(u => u.Id == userId)
                        .AnyAsync(u => u.JoinedAt <= DateTime.UtcNow.AddYears(-1));
                default:
                    return false;

                    // Creativity and Innovation Badges
                    //case BadgeCondition.FoodArtist:
                    //    return await _context.RecipeLikes
                    //        .Where(rl => rl.Recipe.ApplicationUserId == userId && rl.Liked == true)
                    //        .GroupBy(rl => rl.RecipeId)
                    //        .AnyAsync(g => g.Count() >= 10);
                    //case BadgeCondition.RecipeInnovator:
                    //    return await _context.Recipes
                    //        .Where(r => r.ApplicationUserId == userId && r.IsUnique)
                    //        .AnyAsync();

                    // Nutritional and Healthy Living Badges
                    //case BadgeCondition.HealthyChef:
                    //    return await _context.Recipes
                    //        .Where(r => r.ApplicationUserId == userId && r.Tags.Contains("healthy"))
                    //        .CountAsync() >= 10;
                    //case BadgeCondition.CalorieCounter:
                    //    return await _context.RecipeNutritionalAnalysis
                    //        .Where(ra => ra.ApplicationUserId == userId)
                    //        .CountAsync() >= 20;
                    //case BadgeCondition.ConsistentContributor:
                    //    // Check if the user uploaded a recipe every week for a month
                    //    return await _context.Recipes
                    //        .Where(r => r.ApplicationUserId == userId && r.DatePosted >= DateTime.UtcNow.AddMonths(-1))
                    //        .GroupBy(r => new { r.DatePosted.Year, r.DatePosted.Month, r..DayOfYear / 7 })  // Weekly grouping
                    //        .CountAsync(g => g.Count() > 0) >= 4; // At least 1 recipe per week for 4 weeks

                    // Social Interaction Badges
                    //case BadgeCondition.FirstLikeReceived:
                    //    return await _context.RecipeLikes.AnyAsync(rl => rl.Recipe.ApplicationUserId == userId && rl.Liked == true);
                    //case BadgeCondition.SocialButterfly:
                    //    return await _context.RecipeLikes
                    //        .Where(rl => rl.Recipe.ApplicationUserId == userId && rl.Liked == true)
                    //        .GroupBy(rl => rl.UserId)
                    //        .CountAsync() >= 100;
                    //case BadgeCondition.CommenterExtraordinaire:
                    //    return await _context.Comments.CountAsync(c => c.ApplicationUserId == userId) >= 20;
                    //case BadgeCondition.PopularChef:
                    //    return await _context.RecipeLikes
                    //        .Where(rl => rl.Recipe.ApplicationUserId == userId)
                    //        .GroupBy(rl => rl.UserId)
                    //        .CountAsync() >= 50;


                    //// Meal Planning and Organization Badges
                    //case BadgeCondition.PlannerPro:
                    //    return await _context.MealPlans.CountAsync(mp => mp.ApplicationUserId == userId) >= 5;
                    //case BadgeCondition.MasterMealPlanner:
                    //    return await _context.MealPlans
                    //        .Where(mp => mp.ApplicationUserId == userId && mp.PlanDuration == 30)
                    //        .AnyAsync();
            }
        }


        public async Task ProcessUserBadgesAsync(string userId)
        {
            // Fetch all active badges
            var activeBadges = await _context.Badges.Where(b => b.IsActive).ToListAsync();

            foreach (var badge in activeBadges)
            {
                // Check if the condition for the badge is met
                var isConditionMet = await IsConditionMetAsync(userId, badge);

                // Check if the user already has the badge
                var userBadge = await _context.UserBadges
                    .FirstOrDefaultAsync(ub => ub.UserId == userId && ub.BadgeId == badge.Id);

                if (isConditionMet)
                {
                    // Add the badge to the user if not already awarded
                    if (userBadge == null)
                    {
                        var newUserBadge = new UserBadge
                        {
                            UserId = userId,
                            BadgeId = badge.Id,
                            EarnedDate = DateTime.UtcNow,
                            IsActive = true
                            
                        };
                        _context.UserBadges.Add(newUserBadge);
                    }
                }
                else
                {
                    // Remove the badge if the condition is no longer met
                    if (userBadge != null)
                    {
                        _context.UserBadges.Remove(userBadge);
                    }
                }
            }

            // Save changes after processing all badges
            await _context.SaveChangesAsync();
        }


        public async Task<IEnumerable<Badge>> GetUserBadgesAsync(string userId)
        {
            return await _context.UserBadges
                .Where(ub => ub.UserId == userId)
                .Select(ub => ub.Badge)
                .ToListAsync();
        }

        public async Task<Badge> GetBadgeByIdAsync(int id)
        {
            return await _context.Badges.FindAsync(id);
        }

        public async Task AddBadgeAsync(Badge badge)
        {
            _context.Badges.Add(badge);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateBadgeAsync(Badge badge)
        {
            _context.Entry(badge).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteBadgeAsync(int id)
        {
            var badge = await _context.Badges.FindAsync(id);
            if (badge != null)
            {
                _context.Badges.Remove(badge);
                await _context.SaveChangesAsync();
            }
        }
    }

}
