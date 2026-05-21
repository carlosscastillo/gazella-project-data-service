using ProjectDataService.Data.Repositories;
using ProjectDataService.Entities;
using ProjectDataService.Services.Exceptions;

namespace ProjectDataService.Services.Domain;

public static class GazellaValidator
{
    public static async Task<Category> VerifyExistingCategory(
        ICategoryRepository categoryRepository, string categoryId)
    {
        var category = await categoryRepository.GetCategory(categoryId);

        if (category is null)
            throw new GazellaNotFoundException($"No category was found for id: {categoryId}");

        return category;
    }
}