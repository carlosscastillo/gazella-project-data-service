using Microsoft.EntityFrameworkCore;
using Npgsql;
using ProjectDataService.Data.Exceptions;
using ProjectDataService.Entities;
using ProjectDataService.Entities.Interfaces;
using ProjectDataService.Services.Exceptions;

namespace ProjectDataService.Data.Repositories.Implementations;

public class ProjectRepository(GazellaDbContext context, ILogger<ProjectRepository> logger) : IProjectRepository
{
    public async Task<(List<Project> Projects, int TotalCount)> GetProjects(
        int pageIndex, int pageSize, string categoryId, string searchTerm)
    {
        try
        {
            var query = context.Projects
                .Where(p => p.Status == ProjectStatus.Active);

            if (!string.IsNullOrWhiteSpace(categoryId))
                query = query.Where(p => p.Category == categoryId);

            if (!string.IsNullOrWhiteSpace(searchTerm))
                query = query.Where(p => p.Title.Contains(searchTerm));

            var totalCount = await query.CountAsync();
            var projects = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (projects, totalCount);
        }
        catch (Exception ex) when (ex is NpgsqlException || ex is TimeoutException)
        {
            logger.LogError(ex, "Connection exception while getting projects: {Ex}", ex.Message);
            throw new GazellaDbException(ex.Message, ex);
        }
    }

    public async Task<IProject> GetProject(string projectId)
    {
        try
        {
            var project = await context.Projects
                .FirstOrDefaultAsync(p => p.Id == projectId);

            return project ?? (IProject)new NullProject();
        }
        catch (Exception ex) when (ex is NpgsqlException || ex is TimeoutException)
        {
            logger.LogError(ex, "Connection exception while getting project {Id}: {Ex}", projectId, ex.Message);
            throw new GazellaDbException(ex.Message, ex);
        }
    }

    public async Task<List<Project>> GetMyProjects(string organizerId)
    {
        try
        {
            return await context.Projects
                .Where(p => p.OrganizerId == organizerId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex) when (ex is NpgsqlException || ex is TimeoutException)
        {
            logger.LogError(ex, "Connection exception while getting organizer projects: {Ex}", ex.Message);
            throw new GazellaDbException(ex.Message, ex);
        }
    }

    public async Task<(List<Enrollment> Volunteers, int TotalCount)> GetProjectVolunteers(
        string projectId, string organizerId, int pageIndex, int pageSize)
    {
        try
        {
            var project = await context.Projects
                .FirstOrDefaultAsync(p => p.Id == projectId && p.OrganizerId == organizerId);

            if (project is null)
                return ([], 0);

            var query = context.Enrollments
                .Where(e => e.ProjectId == projectId);

            var totalCount = await query.CountAsync();
            var volunteers = await query
                .OrderByDescending(e => e.EnrolledAt)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (volunteers, totalCount);
        }
        catch (Exception ex) when (ex is NpgsqlException || ex is TimeoutException)
        {
            logger.LogError(ex, "Connection exception while getting project volunteers: {Ex}", ex.Message);
            throw new GazellaDbException(ex.Message, ex);
        }
    }

    public async Task<string> CreateProject(Project project)
    {
        try
        {
            await context.Projects.AddAsync(project);
            await context.SaveChangesAsync();
            return project.Id;
        }
        catch (Exception ex) when (ex is NpgsqlException || ex is TimeoutException)
        {
            logger.LogError(ex, "Connection exception while creating project: {Ex}", ex.Message);
            throw new GazellaDbException(ex.Message, ex);
        }
        catch (Exception ex) when (ex is DbUpdateException)
        {
            logger.LogError(ex, "DbUpdate exception while creating project: {Ex}", ex.Message);
            throw new GazellaDbException(ex.Message, ex);
        }
    }

    public async Task UpdateProject(Project project)
    {
        try
        {
            context.Projects.Update(project);
            await context.SaveChangesAsync();
        }
        catch (Exception ex) when (ex is NpgsqlException || ex is TimeoutException)
        {
            logger.LogError(ex, "Connection exception while updating project: {Ex}", ex.Message);
            throw new GazellaDbException(ex.Message, ex);
        }
        catch (Exception ex) when (ex is DbUpdateConcurrencyException)
        {
            logger.LogError(ex, "Concurrency exception while updating project: {Ex}", ex.Message);
            throw new GazellaConcurrencyException(ex.Message);
        }
    }

    public async Task<Project?> GetTrackedProject(string projectId)
    {
        try
        {
            return await context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
        }
        catch (Exception ex) when (ex is NpgsqlException || ex is TimeoutException)
        {
            logger.LogError(ex, "Connection exception while getting tracked project: {Ex}", ex.Message);
            throw new GazellaDbException(ex.Message, ex);
        }
    }
}