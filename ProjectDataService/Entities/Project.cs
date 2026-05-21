using System.ComponentModel.DataAnnotations;
using ProjectDataService.Entities.Interfaces;

namespace ProjectDataService.Entities;

public class Project : IProject
{
    [MaxLength(36)]
    public string Id { get; init; } = Guid.NewGuid().ToString();

    [MaxLength(128)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(256)]
    public string? CoverUri { get; set; }

    [MaxLength(256)]
    public string Location { get; set; } = string.Empty;

    [MaxLength(64)]
    public string Category { get; set; } = string.Empty;

    [MaxLength(36)]
    public string OrganizerId { get; set; } = string.Empty;

    [MaxLength(128)]
    public string OrganizerName { get; set; } = string.Empty;

    [MaxLength(256)]
    public string? OrganizerPfpUri { get; set; }

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int MaxVolunteers { get; set; }
    public int EnrolledCount { get; set; } = 0;
    public ProjectStatus Status { get; set; } = ProjectStatus.Draft;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public List<Enrollment> Enrollments { get; set; } = [];
}