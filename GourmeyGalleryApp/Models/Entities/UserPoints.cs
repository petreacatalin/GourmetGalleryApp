namespace GourmeyGalleryApp.Models.Entities
{
    public class UserPoints
    {
        public int Id { get; set; }
        public string UserId { get; set; } // Link to Identity User
        public int Points { get; set; }
    }

}
