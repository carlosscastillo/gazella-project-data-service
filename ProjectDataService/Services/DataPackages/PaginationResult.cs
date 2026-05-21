namespace ProjectDataService.Services.DataPackages;

public record PaginationResult(int TotalCount, int PageCount, int CurrentPage, int PageSize);