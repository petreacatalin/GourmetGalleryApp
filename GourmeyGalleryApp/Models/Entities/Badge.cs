namespace GourmeyGalleryApp.Models.Entities
{

    public class Badge
    {
        public int Id { get; set; }
        public string Name { get; set; }  // e.g., "First Recipe Posted"
        public string Description { get; set; }  // Badge description
        public string IconUrl { get; set; }  // URL for the badge icon
        public int Points { get; set; }  // Points awarded for the badge
        public BadgeCondition Condition { get; set; }  // Condition to trigger badge (e.g., "First Recipe", "5 Recipes")
        public bool IsActive { get; set; } = true;  // If the badge is active or not
    }
    public enum BadgeCondition
    {
        // User Engagement Badges
        FirstRecipePosted,          // Awarded to users who post their very first recipe. -     10
        RecipeEnthusiast,           // Given to users who post multiple recipes.          -      25
        RecipeMaster,               // Earned by users who post a significant number of high-quality recipes.       -50
        ConsistentContributor,      // Given to users who consistently contribute over time.       -30

        // Social Interaction Badges
        FirstLikeReceived,          // Awarded to users who receive their first "like" on a recipe or post.       -5
        SocialButterfly,            // Given to users who engage widely with others in the community.       -20
        CommenterExtraordinaire,    // Earned by users who leave thoughtful comments on multiple posts.       -15
        PopularChef,                // Awarded to users whose recipes or posts receive a large number of likes.       -40

        // Cooking Challenges Badges
        ChallengeWinner,            // Given to users who win cooking challenges.       -50
        ChallengeParticipant,       // Earned by users who participate in cooking challenges.       -15
        IronChef,                   // A prestigious badge awarded to top chefs in challenges.       -100

        // Meal Planning and Organization Badges
        PlannerPro,                 // Given to users who demonstrate exceptional meal planning skills.       -30
        MasterMealPlanner,          // Awarded to users who regularly plan and share meal plans.       -50

        // Milestone and Loyalty Badges
        RecipeExplorer,             // Earned by users who explore diverse recipes and ingredients.       -20
        EarlyAdopter,               // Given to users who joined the platform early and contributed.       -40
        OneYearAnniversary,         // Awarded to users who have been active for one full year.       -100

        // Creativity and Innovation Badges
        FoodArtist,                 // Awarded to users who create visually stunning recipes.       -35
        RecipeInnovator,            // Given to users who create unique and original recipes.       -50

        // Nutritional and Healthy Living Badges
        HealthyChef,                // Awarded to users who consistently create healthy recipes.       -30
        CalorieCounter              // Given to users who track and share the nutritional value of their recipes.       -10
    }


}
