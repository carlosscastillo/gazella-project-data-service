using Grpc.Core;
using ProjectDataService.Data.Repositories;
using ProjectDataService.Entities;
using ProjectDataService.Protos;
using ProjectDataService.Services.Domain;
using ProjectDataService.Services.Exceptions;
using ProjectDataService.Services.MessageValidators;

namespace ProjectDataService.Services;

public class ProjectManagementService(
    IProjectRepository projectRepository,
    ICategoryRepository categoryRepository) : Protos.ProjectManagementService.ProjectManagementServiceBase
{
    public override async Task<CreateProjectResponse> CreateProject(
        CreateProjectRequest request, ServerCallContext context)
    {
        ManagementValidator.ValidateCreateProject(request);
        var category = await GazellaValidator.VerifyExistingCategory(categoryRepository, request.CategoryId);

        var status = request.IsDraft ? ProjectStatus.Draft : ProjectStatus.Active;

        var project = new Project
        {
            Title = request.Title,
            Description = request.Description,
            CoverUri = string.IsNullOrWhiteSpace(request.CoverUri) ? null : request.CoverUri,
            Location = request.Location,
            Category = category.Name,
            OrganizerId = request.OrganizerId,
            OrganizerName = request.OrganizerName,
            OrganizerPfpUri = string.IsNullOrWhiteSpace(request.OrganizerPfpUri) ? null : request.OrganizerPfpUri,
            StartDate = DateTime.Parse(request.StartDate).ToUniversalTime(),
            EndDate = DateTime.Parse(request.EndDate).ToUniversalTime(),
            MaxVolunteers = request.MaxVolunteers,
            Status = status
        };

        var projectId = await projectRepository.CreateProject(project);

        return new CreateProjectResponse
        {
            ProjectId = projectId,
            Message = request.IsDraft ? "Project saved as draft" : "Project created successfully"
        };
    }

    public override async Task<UpdateProjectResponse> UpdateProject(
        UpdateProjectRequest request, ServerCallContext context)
    {
        ManagementValidator.ValidateUpdateProject(request);
        var category = await GazellaValidator.VerifyExistingCategory(categoryRepository, request.CategoryId);

        var existing = await projectRepository.GetTrackedProject(request.ProjectId);

        if (existing is null)
            throw new GazellaNotFoundException($"No project was found for id: {request.ProjectId}");

        if (existing.OrganizerId != request.OrganizerId)
            throw new GazellaInvalidOperationException("You are not the organizer of this project");

        if (existing.Status == ProjectStatus.Cancelled)
            throw new GazellaInvalidOperationException("Cancelled projects cannot be updated");

        existing.Title = request.Title;
        existing.Description = request.Description;
        existing.CoverUri = string.IsNullOrWhiteSpace(request.CoverUri) ? null : request.CoverUri;
        existing.Location = request.Location;
        existing.Category = category.Name;
        existing.StartDate = DateTime.Parse(request.StartDate).ToUniversalTime();
        existing.EndDate = DateTime.Parse(request.EndDate).ToUniversalTime();
        existing.MaxVolunteers = request.MaxVolunteers;
        existing.UpdatedAt = DateTime.UtcNow;

        await projectRepository.UpdateProject(existing);

        return new UpdateProjectResponse
        {
            IsSuccess = true,
            Message = "Project updated successfully"
        };
    }

    public override async Task<CancelProjectResponse> CancelProject(
        CancelProjectRequest request, ServerCallContext context)
    {
        GeneralValidator.ValidateId(request.ProjectId, "project_id");
        GeneralValidator.ValidateId(request.OrganizerId, "organizer_id");

        var existing = await projectRepository.GetTrackedProject(request.ProjectId);

        if (existing is null)
            throw new GazellaNotFoundException($"No project was found for id: {request.ProjectId}");

        if (existing.OrganizerId != request.OrganizerId)
            throw new GazellaInvalidOperationException("You are not the organizer of this project");

        if (existing.Status == ProjectStatus.Cancelled)
            throw new GazellaInvalidOperationException("Project is already cancelled");

        existing.Status = ProjectStatus.Cancelled;
        existing.UpdatedAt = DateTime.UtcNow;

        await projectRepository.UpdateProject(existing);

        return new CancelProjectResponse
        {
            ProjectStatus = existing.Status.ToString(),
            Message = "Project cancelled successfully"
        };
    }
}