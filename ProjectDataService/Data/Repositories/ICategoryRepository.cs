using ProjectDataService.Entities;

namespace ProjectDataService.Data.Repositories;

public interface ICategoryRepository
{
    Task<Category?> GetCategory(string categoryId);
}