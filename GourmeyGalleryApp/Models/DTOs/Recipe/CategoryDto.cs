using GourmeyGalleryApp.Models.Entities;
using System.Text.Json.Serialization;

namespace GourmeyGalleryApp.Models.DTOs.Recipe
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Slug { get; set; }
        public int? ParentCategoryId { get; set; }
        public List<CategoryDto> Subcategories { get; set; } = new List<CategoryDto>();
    }
}
