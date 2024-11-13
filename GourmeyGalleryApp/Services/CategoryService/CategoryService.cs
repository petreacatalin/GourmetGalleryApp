using GourmeyGalleryApp.Models.Entities;
using GourmeyGalleryApp.Repositories.CategoryRepository;
using GourmeyGalleryApp.Services.CategoryService;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;

    public CategoryService(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }
    public async Task<IEnumerable<Category>> GetTopLevelCategoriesAsync()
    {
        return await _categoryRepository.GetTopLevelCategoriesAsync();
    }

    public async Task<IEnumerable<Category>> GetSubcategoriesAsync(int categoryId)
    {
        return await _categoryRepository.GetSubcategoriesAsync(categoryId);
    }
    public async Task<List<Category>> GetAllCategoriesAsync()
    {
        return await _categoryRepository.GetAllCategoriesAsync();
    }

    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        return await _categoryRepository.GetCategoryByIdAsync(id);
    }

    public async Task AddCategoryAsync(Category category)
    {
        category.Slug = GenerateSlug(category.Name);
        await _categoryRepository.AddCategoryAsync(category);
    }
    public async Task UpdateCategoryAsync(Category category)
    {
        // Optionally, generate a slug here
        category.Slug = GenerateSlug(category.Name);
        await _categoryRepository.UpdateCategoryAsync(category);
    }

    public async Task DeleteCategoryAsync(int id)
    {
        await _categoryRepository.DeleteCategoryAsync(id);
    }

    private string GenerateSlug(string name)
    {
        return name.ToLower().Replace(" ", "-").Replace(",", "").Replace("&", "and");
    }
}
