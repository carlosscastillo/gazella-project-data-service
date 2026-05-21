namespace ProjectDataService.Services.DataPackages;

public static class PaginationUtil
{
    public static PaginationResult Calculate(int totalCount, int pageIndex, int pageSize)
    {
        var pageCount = totalCount == 0 ? 0 : (int)Math.Ceiling((double)totalCount / pageSize);
        return new PaginationResult(totalCount, pageCount, pageIndex, pageSize);
    }
}