using ProjectDataService.Services.Exceptions;

namespace ProjectDataService.Services.MessageValidators;

public static class GeneralValidator
{
    public static void ValidateId(string id, string fieldName = "id")
    {
        var issues = new List<string>();

        if (string.IsNullOrWhiteSpace(id))
            issues.Add($"{fieldName} is required");
        else if (!Guid.TryParse(id, out _))
            issues.Add($"{fieldName} must be a valid UUID");

        if (issues.Count > 0)
            throw new GazellaValidationException(ExceptionUtil.IssueStringify(issues));
    }
}