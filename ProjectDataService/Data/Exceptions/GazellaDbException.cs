namespace ProjectDataService.Data.Exceptions;

public class GazellaDbException(string message, Exception innerException)
    : Exception(message, innerException) { }