using Grpc.Core;
using ProjectDataService.Data.Repositories;
using ProjectDataService.Entities;
using ProjectDataService.Protos;
using ProjectDataService.Services.DataPackages;
using ProjectDataService.Services.Exceptions;
using ProjectDataService.Services.MessageValidators;

namespace ProjectDataService.Services;

public class ProjectQueryService(
    IProjectRepository projectRepository) : Protos.ProjectService.ProjectServiceBase
{
    public override async Task<GetProjectsResponse> GetProjects(
        GetProjectsRequest request, ServerCallContext context)
    {
        var (projects, totalCount) = await projectRepository.GetProjects(
            request.PageIndex == 0 ? 1 : request.PageIndex,
            request.PageSize == 0 ? 10 : request.PageSize,
            request.CategoryId,
            request.SearchTerm,
            request.Location,
            request.StartDate,
            request.OrderBy);

        var pagination = PaginationUtil.Calculate(
            totalCount,
            request.PageIndex == 0 ? 1 : request.PageIndex,
            request.PageSize == 0 ? 10 : request.PageSize);

        var response = new GetProjectsResponse
        {
            TotalProjects = totalCount,
            CurrentPage = pagination.CurrentPage,
            PageCount = pagination.PageCount,
            PageSize = pagination.PageSize
        };

        response.Projects.AddRange(projects.Select(p => new ProjectSummary
        {
            ProjectId = p.Id,
            Title = p.Title,
            Description = p.Description,
            CoverUri = p.CoverUri ?? string.Empty,
            Location = p.Location,
            Category = p.Category,
            StartDate = p.StartDate.ToString("yyyy-MM-dd"),
            EndDate = p.EndDate.ToString("yyyy-MM-dd"),
            Status = p.Status.ToString(),
            EnrolledCount = p.EnrolledCount,
            MaxVolunteers = p.MaxVolunteers
        }));

        return response;
    }

    public override async Task<GetProjectResponse> GetProject(
        GetProjectRequest request, ServerCallContext context)
    {
        GeneralValidator.ValidateId(request.ProjectId, "project_id");

        var project = await projectRepository.GetProject(request.ProjectId);

        if (string.IsNullOrEmpty(project.Id))
            throw new GazellaNotFoundException($"No project was found for id: {request.ProjectId}");

        return new GetProjectResponse
        {
            ProjectId = project.Id,
            Title = project.Title,
            Description = project.Description,
            CoverUri = project.CoverUri ?? string.Empty,
            Location = project.Location,
            Category = project.Category,
            StartDate = project.StartDate.ToString("yyyy-MM-dd"),
            EndDate = project.EndDate.ToString("yyyy-MM-dd"),
            Status = project.Status.ToString(),
            EnrolledCount = project.EnrolledCount,
            MaxVolunteers = project.MaxVolunteers,
            OrganizerId = project.OrganizerId,
            OrganizerName = project.OrganizerName,
            OrganizerPfpUri = project.OrganizerPfpUri ?? string.Empty,
            CreatedAt = project.CreatedAt.ToString("o")
        };
    }

    public override async Task<GetMyProjectsResponse> GetMyProjects(
        GetMyProjectsRequest request, ServerCallContext context)
    {
        GeneralValidator.ValidateId(request.OrganizerId, "organizer_id");

        var projects = await projectRepository.GetMyProjects(request.OrganizerId);

        var response = new GetMyProjectsResponse();
        response.Projects.AddRange(projects.Select(p => new MyProject
        {
            ProjectId = p.Id,
            Title = p.Title,
            Location = p.Location,
            StartDate = p.StartDate.ToString("yyyy-MM-dd"),
            EndDate = p.EndDate.ToString("yyyy-MM-dd"),
            Status = p.Status.ToString(),
            EnrolledCount = p.EnrolledCount,
            MaxVolunteers = p.MaxVolunteers,
            CoverUri = p.CoverUri ?? string.Empty
        }));

        return response;
    }

    public override async Task<GetProjectVolunteersResponse> GetProjectVolunteers(
        GetProjectVolunteersRequest request, ServerCallContext context)
    {
        GeneralValidator.ValidateId(request.ProjectId, "project_id");
        GeneralValidator.ValidateId(request.OrganizerId, "organizer_id");

        var pageIndex = request.PageIndex == 0 ? 1 : request.PageIndex;
        var pageSize = request.PageSize == 0 ? 10 : request.PageSize;

        var (volunteers, totalCount) = await projectRepository.GetProjectVolunteers(
            request.ProjectId, request.OrganizerId, pageIndex, pageSize,
            request.SearchTerm, request.StatusFilter);

        var pagination = PaginationUtil.Calculate(totalCount, pageIndex, pageSize);

        var response = new GetProjectVolunteersResponse
        {
            TotalVolunteers = totalCount,
            CurrentPage = pagination.CurrentPage,
            PageCount = pagination.PageCount,
            PageSize = pagination.PageSize
        };

        response.Volunteers.AddRange(volunteers.Select(v => new EnrolledVolunteer
        {
            VolunteerId = v.VolunteerId,
            FullName = v.VolunteerFullName,
            Email = v.VolunteerEmail,
            EnrolledAt = v.EnrolledAt.ToString("o"),
            EnrollmentStatus = v.Status.ToString()
        }));

        return response;
    }
}