namespace Application.Common
{
    public sealed record PaginatedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);
}
