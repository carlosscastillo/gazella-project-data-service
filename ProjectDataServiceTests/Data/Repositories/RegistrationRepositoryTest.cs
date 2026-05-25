using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using ProjectDataService.Data;
using ProjectDataService.Data.Repositories.Implementations;
using ProjectDataService.Entities;
using ProjectDataServiceTests.Data;
using Testcontainers.PostgreSql;

namespace ProjectDataServiceTests.Data.Repositories;

[TestFixture]
public class RegistrationRepositoryTest
{
    private PostgreSqlContainer _container = null!;
    private GazellaDbContext _context = null!;
    private RegistrationRepository _repository = null!;
    private string _projectId = null!;

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

        _repository = new RegistrationRepository(_context, new NullLogger<RegistrationRepository>());

        var project = new Project
        {
            Title = "Proyecto base", Description = "Desc", Location = "Xalapa",
            Category = "Bio", OrganizerId = "org-1", OrganizerName = "Org",
            StartDate = DateTime.UtcNow.AddDays(5), EndDate = DateTime.UtcNow.AddDays(6),
            MaxVolunteers = 20, Status = ProjectStatus.Active
        };
        await _context.Projects.AddAsync(project);
        await _context.SaveChangesAsync();
        _projectId = project.Id;
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
        _context.Enrollments.RemoveRange(_context.Enrollments);
        await _context.SaveChangesAsync();
    }

    private Enrollment CreateTestEnrollment(string volunteerId = "vol-1")
    {
        return new Enrollment
        {
            ProjectId = _projectId,
            VolunteerId = volunteerId,
            VolunteerFullName = "Voluntario Prueba",
            VolunteerEmail = "voluntario@test.com",
            Status = EnrollmentStatus.Confirmed
        };
    }

    [Test]
    public async Task CreateEnrollment_ValidEnrollment_ReturnsId()
    {
        var enrollment = CreateTestEnrollment("vol-create");

        var id = await _repository.CreateEnrollment(enrollment);

        Assert.That(id, Is.Not.Null.Or.Empty);
        Assert.That(id, Is.EqualTo(enrollment.Id));
    }

    [Test]
    public async Task GetTrackedEnrollment_ExistingEnrollment_ReturnsEnrollment()
    {
        var enrollment = CreateTestEnrollment("vol-get");
        await _repository.CreateEnrollment(enrollment);

        var result = await _repository.GetTrackedEnrollment(_projectId, "vol-get");

        Assert.That(result, Is.Not.Null);
        Assert.That(result!.VolunteerId, Is.EqualTo("vol-get"));
    }

    [Test]
    public async Task GetTrackedEnrollment_NonExisting_ReturnsNull()
    {
        var result = await _repository.GetTrackedEnrollment(_projectId, "vol-nonexistent");

        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task UpdateEnrollment_CancelEnrollment_PersistsStatus()
    {
        var enrollment = CreateTestEnrollment("vol-update");
        await _repository.CreateEnrollment(enrollment);

        enrollment.Status = EnrollmentStatus.Cancelled;
        await _repository.UpdateEnrollment(enrollment);

        var updated = await _repository.GetTrackedEnrollment(_projectId, "vol-update");
        Assert.That(updated!.Status, Is.EqualTo(EnrollmentStatus.Cancelled));
    }

    [Test]
    public async Task GetMyEnrollments_ReturnsOnlyVolunteerEnrollments()
    {
        var mine = CreateTestEnrollment("vol-mine");
        var other = CreateTestEnrollment("vol-other");
        await _repository.CreateEnrollment(mine);
        await _repository.CreateEnrollment(other);

        var result = await _repository.GetMyEnrollments("vol-mine");

        Assert.That(result.Count, Is.EqualTo(1));
        Assert.That(result[0].VolunteerId, Is.EqualTo("vol-mine"));
    }

    [Test]
    public async Task GetMyEnrollments_IncludesProjectData()
    {
        var enrollment = CreateTestEnrollment("vol-include");
        await _repository.CreateEnrollment(enrollment);

        var result = await _repository.GetMyEnrollments("vol-include");

        Assert.That(result[0].Project, Is.Not.Null);
        Assert.That(result[0].Project!.Title, Is.EqualTo("Proyecto base"));
    }
}