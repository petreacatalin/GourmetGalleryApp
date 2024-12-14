using GourmeyGalleryApp.Models.Entities;

namespace GourmeyGalleryApp.Models.DTOs.ApplicationUser
{
    public class ApplicationUserDto
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? Email { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? About { get; set; }

        public DateTime JoinedAt { get; set; }
        public List<BadgeDto>? Badges { get; set; } = new List<BadgeDto>();

    }
}
