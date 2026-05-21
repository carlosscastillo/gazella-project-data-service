using System.ComponentModel.DataAnnotations;
using ProjectDataService.Entities.Interfaces;

namespace ProjectDataService.Entities;

public class Enrollment : IEnrollment
{
    [MaxLength(36)]
    public string Id { get; init; } = Guid.NewGuid().ToString();

    [MaxLength(36)]
    public string ProjectId { get; set; } = string.Empty;

    [MaxLength(36)]
    public string VolunteerId { get; set; } = string.Empty;

    [MaxLength(128)]
    public string VolunteerFullName { get; set; } = string.Empty;

    [MaxLength(256)]
    public string VolunteerEmail { get; set; } = string.Empty;

    public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Confirmed;
    public DateTime EnrolledAt { get; init; } = DateTime.UtcNow;

    public Project? Project { get; set; }
}