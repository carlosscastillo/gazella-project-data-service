using Microsoft.EntityFrameworkCore;
using Npgsql;
using ProjectDataService.Data.Exceptions;
using ProjectDataService.Entities;

namespace ProjectDataService.Data.Repositories.Implementations;

public class CategoryRepository(GazellaDbContext context, ILogger<CategoryRepository> logger) : ICategoryRepository
{
    public async Task<Category?> GetCategory(string categoryId)
    {
        try
        {
            return await context.Categories.FirstOrDefaultAsync(c => c.Id == categoryId);
        }
        catch (Exception ex) when (ex is NpgsqlException || ex is TimeoutException)
        {
            logger.LogError(ex, "Connection exception while getting category: {Ex}", ex.Message);
            throw new GazellaDbException(ex.Message, ex);
        }
    }
}