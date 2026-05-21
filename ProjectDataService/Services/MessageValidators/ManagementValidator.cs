using ProjectDataService.Protos;
using ProjectDataService.Services.Exceptions;

namespace ProjectDataService.Services.MessageValidators;

public static class ManagementValidator
{
    public static void ValidateCreateProject(CreateProjectRequest request)
    {
        var issues = new List<string>();

        if (string.IsNullOrWhiteSpace(request.Title))
            issues.Add("title is required");
        else if (request.Title.Length > 128)
            issues.Add("title cannot exceed 128 characters");

        if (string.IsNullOrWhiteSpace(request.Description))
            issues.Add("description is required");
        else if (request.Description.Length > 2000)
            issues.Add("description cannot exceed 2000 characters");

        if (string.IsNullOrWhiteSpace(request.Location))
            issues.Add("location is required");
        else if (request.Location.Length > 256)
            issues.Add("location cannot exceed 256 characters");

        if (string.IsNullOrWhiteSpace(request.OrganizerId))
            issues.Add("organizer_id is required");

        if (string.IsNullOrWhiteSpace(request.CategoryId))
            issues.Add("category_id is required");

        if (string.IsNullOrWhiteSpace(request.StartDate))
            issues.Add("start_date is required");

        if (string.IsNullOrWhiteSpace(request.EndDate))
            issues.Add("end_date is required");

        if (request.MaxVolunteers < 1)
            issues.Add("max_volunteers must be at least 1");

        if (!string.IsNullOrWhiteSpace(request.StartDate) &&
            !string.IsNullOrWhiteSpace(request.EndDate) &&
            DateTime.TryParse(request.StartDate, out var start) &&
            DateTime.TryParse(request.EndDate, out var end) &&
            end <= start)
            issues.Add("end_date must be after start_date");

        if (issues.Count > 0)
            throw new GazellaValidationException(ExceptionUtil.IssueStringify(issues));
    }

    public static void ValidateUpdateProject(UpdateProjectRequest request)
    {
        var issues = new List<string>();

        if (string.IsNullOrWhiteSpace(request.ProjectId))
            issues.Add("project_id is required");

        if (string.IsNullOrWhiteSpace(request.OrganizerId))
            issues.Add("organizer_id is required");

        if (string.IsNullOrWhiteSpace(request.Title))
            issues.Add("title is required");
        else if (request.Title.Length > 128)
            issues.Add("title cannot exceed 128 characters");

        if (string.IsNullOrWhiteSpace(request.Description))
            issues.Add("description is required");
        else if (request.Description.Length > 2000)
            issues.Add("description cannot exceed 2000 characters");

        if (string.IsNullOrWhiteSpace(request.Location))
            issues.Add("location is required");

        if (!string.IsNullOrWhiteSpace(request.StartDate) &&
            !string.IsNullOrWhiteSpace(request.EndDate) &&
            DateTime.TryParse(request.StartDate, out var start) &&
            DateTime.TryParse(request.EndDate, out var end) &&
            end <= start)
            issues.Add("end_date must be after start_date");

        if (request.MaxVolunteers < 1)
            issues.Add("max_volunteers must be at least 1");

        if (issues.Count > 0)
            throw new GazellaValidationException(ExceptionUtil.IssueStringify(issues));
    }
}