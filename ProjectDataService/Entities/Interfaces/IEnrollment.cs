namespace ProjectDataService.Entities.Interfaces;

public interface IEnrollment
{
    string Id { get; }
    string ProjectId { get; }
    string VolunteerId { get; }
    string VolunteerFullName { get; }
    string VolunteerEmail { get; }
    EnrollmentStatus Status { get; }
    DateTime EnrolledAt { get; }
}