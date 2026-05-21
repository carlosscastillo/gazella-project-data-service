using ProjectDataService.Protos;
using ProjectDataService.Services.Exceptions;

namespace ProjectDataService.Services.MessageValidators;

public static class RegistrationValidator
{
    public static void ValidateSignUp(SignUpRequest request)
    {
        var issues = new List<string>();

        if (string.IsNullOrWhiteSpace(request.ProjectId))
            issues.Add("project_id is required");

        if (string.IsNullOrWhiteSpace(request.VolunteerId))
            issues.Add("volunteer_id is required");

        if (issues.Count > 0)
            throw new GazellaValidationException(ExceptionUtil.IssueStringify(issues));
    }

    public static void ValidateCancelRegistration(CancelRegistrationRequest request)
    {
        var issues = new List<string>();

        if (string.IsNullOrWhiteSpace(request.ProjectId))
            issues.Add("project_id is required");

        if (string.IsNullOrWhiteSpace(request.VolunteerId))
            issues.Add("volunteer_id is required");

        if (issues.Count > 0)
            throw new GazellaValidationException(ExceptionUtil.IssueStringify(issues));
    }
}