namespace ProjectDataService.Services.Exceptions;

public class GazellaConcurrencyException(string message) : Exception(message) { }