namespace ProjectDataService.Services.Exceptions;

public class GazellaValidationException(string issues) : Exception(issues)
{
    public string Issues { get; } = issues;
}