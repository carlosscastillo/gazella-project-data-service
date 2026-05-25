using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ProjectDataService.Data;
using ProjectDataService.Data.Repositories.Implementations;
using ProjectDataService.Entities;
using ProjectDataServiceTests.Data;
using Testcontainers.PostgreSql;

namespace ProjectDataServiceTests.Data.Repositories;

[TestFixture]
public class CategoryRepositoryTest
{
    private PostgreSqlContainer _container = null!;
    private GazellaDbContext _context = null!;
    private CategoryRepository _repository = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetup()
    {
        _container = new PostgreSqlBuilder()
            .WithImage(DbConstants.DbImage)
            .WithDatabase(DbConstants.DbName)
            .WithUsername(DbConstants.DbUser)
            .WithPassword(DbConstants.DbPassword)
            .Build();

        await _container.StartAsync();

        var options = new DbContextOptionsBuilder<GazellaDbContext>()
            .UseNpgsql(_container.GetConnectionString())
            .Options;

        _context = new GazellaDbContext(options);
        await _context.Database.EnsureCreatedAsync();

        _repository = new CategoryRepository(_context, new NullLogger<CategoryRepository>());
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _context.DisposeAsync();
        await _container.StopAsync();
        await _container.DisposeAsync();
    }

    [TearDown]
    public async Task TearDown()
    {
        _context.Categories.RemoveRange(_context.Categories);
        await _context.SaveChangesAsync();
    }

    [Test]
    public async Task GetCategory_ExistingId_ReturnsCategory()
    {
        var category = new Category { Name = "Biodiversidad" };
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();

        var result = await _repository.GetCategory(category.Id);

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Id, Is.EqualTo(category.Id));
        Assert.That(result.Name, Is.EqualTo("Biodiversidad"));
    }

    [Test]
    public async Task GetCategory_NonExistingId_ReturnsNull()
    {
        var result = await _repository.GetCategory(Guid.NewGuid().ToString());

        Assert.That(result, Is.Null);
    }
}