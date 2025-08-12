using Domain.Enums;

namespace Api.Dtos
{
    public sealed record ListActivitiesQueryParams(
    DateTime? DateFromUtc,
    DateTime? DateToUtc,
    ActivityType[]? Types,
    int Page = 1,
    int PageSize = 20
);
}
