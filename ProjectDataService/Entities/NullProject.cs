using ProjectDataService.Entities.Interfaces;

namespace ProjectDataService.Entities;

public class NullProject : IProject
{
    public string Id => string.Empty;
    public string Title => string.Empty;
    public string Description => string.Empty;
    public string? CoverUri => null;
    public string Location => string.Empty;
    public string Category => string.Empty;
    public string OrganizerId => string.Empty;
    public string OrganizerName => string.Empty;
    public string? OrganizerPfpUri => null;
    public DateTime StartDate => default;
    public DateTime EndDate => default;
    public int MaxVolunteers => 0;
    public int EnrolledCount => 0;
    public ProjectStatus Status => ProjectStatus.Draft;
    public DateTime CreatedAt => default;
    public DateTime? UpdatedAt => null;
}