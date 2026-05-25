using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ProjectDataService.Data;
using ProjectDataService.Data.Repositories.Implementations;
using ProjectDataService.Entities;
using ProjectDataServiceTests.Data;
using Testcontainers.PostgreSql;

namespace ProjectDataServiceTests.Data.Repositories;

[TestFixture]
public class ProjectRepositoryTest
{
    private PostgreSqlContainer _container = null!;
    private GazellaDbContext _context = null!;
    private ProjectRepository _repository = null!;

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

        _repository = new ProjectRepository(_context, new NullLogger<ProjectRepository>());
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
        _context.Projects.RemoveRange(_context.Projects);
        await _context.SaveChangesAsync();
    }

    private static Project CreateTestProject(string organizerId = "org-1", string location = "Xalapa", ProjectStatus status = ProjectStatus.Active)
    {
        return new Project
        {
            Title = "Proyecto de prueba",
            Description = "Descripción de prueba",
            Location = location,
            Category = "Biodiversidad",
            OrganizerId = organizerId,
            OrganizerName = "Organizador Prueba",
            StartDate = DateTime.UtcNow.AddDays(10),
            EndDate = DateTime.UtcNow.AddDays(11),
            MaxVolunteers = 20,
            Status = status
        };
    }

    [Test]
    public async Task CreateProject_ValidProject_ReturnsId()
    {
        var project = CreateTestProject();

        var id = await _repository.CreateProject(project);

        Assert.That(id, Is.Not.Null.Or.Empty);
        Assert.That(id, Is.EqualTo(project.Id));
    }

    [Test]
    public async Task GetProject_ExistingId_ReturnsProject()
    {
        var project = CreateTestProject();
        await _repository.CreateProject(project);

        var result = await _repository.GetProject(project.Id);

        Assert.That(result.Id, Is.EqualTo(project.Id));
        Assert.That(result.Title, Is.EqualTo("Proyecto de prueba"));
    }

    [Test]
    public async Task GetProject_NonExistingId_ReturnsNullProject()
    {
        var result = await _repository.GetProject(Guid.NewGuid().ToString());

        Assert.That(result.Id, Is.EqualTo(string.Empty));
    }

    [Test]
    public async Task GetProjects_NoFilters_ReturnsActiveProjects()
    {
        var active = CreateTestProject(status: ProjectStatus.Active);
        var draft = CreateTestProject(status: ProjectStatus.Draft);
        await _repository.CreateProject(active);
        await _repository.CreateProject(draft);

        var (projects, totalCount) = await _repository.GetProjects(1, 10, "", "", "", "", "newest");

        Assert.That(totalCount, Is.EqualTo(1));
        Assert.That(projects[0].Id, Is.EqualTo(active.Id));
    }

    [Test]
    public async Task GetProjects_FilterByLocation_ReturnsMatchingProjects()
    {
        var xalapa = CreateTestProject(location: "Xalapa, Veracruz");
        var veracruz = CreateTestProject(location: "Veracruz, Puerto");
        await _repository.CreateProject(xalapa);
        await _repository.CreateProject(veracruz);

        var (projects, totalCount) = await _repository.GetProjects(1, 10, "", "", "Xalapa", "", "newest");

        Assert.That(totalCount, Is.EqualTo(1));
        Assert.That(projects[0].Location, Does.Contain("Xalapa"));
    }

    [Test]
    public async Task GetProjects_OrderBySoonest_ReturnsSortedByStartDate()
    {
        var later = new Project
        {
            Title = "Proyecto Lejano", Description = "Desc", Location = "Xalapa",
            Category = "Bio", OrganizerId = "org-1", OrganizerName = "Org",
            StartDate = DateTime.UtcNow.AddDays(30), EndDate = DateTime.UtcNow.AddDays(31),
            MaxVolunteers = 10, Status = ProjectStatus.Active
        };
        var sooner = new Project
        {
            Title = "Proyecto Próximo", Description = "Desc", Location = "Xalapa",
            Category = "Bio", OrganizerId = "org-1", OrganizerName = "Org",
            StartDate = DateTime.UtcNow.AddDays(5), EndDate = DateTime.UtcNow.AddDays(6),
            MaxVolunteers = 10, Status = ProjectStatus.Active
        };
        await _repository.CreateProject(later);
        await _repository.CreateProject(sooner);

        var (projects, _) = await _repository.GetProjects(1, 10, "", "", "", "", "soonest");

        Assert.That(projects[0].Title, Is.EqualTo("Proyecto Próximo"));
    }

    [Test]
    public async Task GetMyProjects_ReturnsOnlyOrganizerProjects()
    {
        var mine = CreateTestProject(organizerId: "organizer-abc");
        var other = CreateTestProject(organizerId: "organizer-xyz");
        await _repository.CreateProject(mine);
        await _repository.CreateProject(other);

        var result = await _repository.GetMyProjects("organizer-abc");

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].OrganizerId, Is.EqualTo("organizer-abc"));
    }

    [Test]
    public async Task UpdateProject_ValidChanges_PersistsUpdates()
    {
        var project = CreateTestProject();
        await _repository.CreateProject(project);

        project.Title = "Título actualizado";
        project.MaxVolunteers = 50;
        await _repository.UpdateProject(project);

        var updated = await _repository.GetTrackedProject(project.Id);
        Assert.That(updated!.Title, Is.EqualTo("Título actualizado"));
        Assert.That(updated.MaxVolunteers, Is.EqualTo(50));
    }

    [Test]
    public async Task GetTrackedProject_NonExistingId_ReturnsNull()
    {
        var result = await _repository.GetTrackedProject(Guid.NewGuid().ToString());

        Assert.That(result, Is.Null);
    }
}