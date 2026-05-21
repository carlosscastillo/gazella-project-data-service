using ProjectDataService.Entities;
using ProjectDataService.Entities.Interfaces;

namespace ProjectDataService.Data.Repositories;

public interface IProjectRepository
{
    Task<(List<Project> Projects, int TotalCount)> GetProjects(int pageIndex, int pageSize, string categoryId, string searchTerm);
    Task<IProject> GetProject(string projectId);
    Task<List<Project>> GetMyProjects(string organizerId);
    Task<(List<Enrollment> Volunteers, int TotalCount)> GetProjectVolunteers(string projectId, string organizerId, int pageIndex, int pageSize);
    Task<string> CreateProject(Project project);
    Task UpdateProject(Project project);
    Task<Project?> GetTrackedProject(string projectId);
}