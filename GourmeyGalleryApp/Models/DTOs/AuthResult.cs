namespace GourmeyGalleryApp.Models.DTOs
{
    public class AuthResult
    {
        public string? Token { get; set; } 
        public bool Result { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public string? Message { get; set; }
    }
}
