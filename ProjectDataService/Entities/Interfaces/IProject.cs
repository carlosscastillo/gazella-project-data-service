namespace ProjectDataService.Entities.Interfaces;

public interface IProject
{
    string Id { get; }
    string Title { get; }
    string Description { get; }
    string? CoverUri { get; }
    string Location { get; }
    string Category { get; }
    string OrganizerId { get; }
    string OrganizerName { get; }
    string? OrganizerPfpUri { get; }
    DateTime StartDate { get; }
    DateTime EndDate { get; }
    int MaxVolunteers { get; }
    int EnrolledCount { get; }
    ProjectStatus Status { get; }
    DateTime CreatedAt { get; }
    DateTime? UpdatedAt { get; }
}