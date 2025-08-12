using Domain.Entities;
using Domain.Enums;
using Application.Common;

namespace Application.Abstractions
{
    public sealed record ActivityFilter(
    Guid UserId,
    DateTime? DateFromUtc = null,
    DateTime? DateToUtc = null,
    ActivityType[]? Types = null,
    int Page = 1,
    int PageSize = 20);

    public interface IActivityRepository
    {
        Task<Activity?> GetByIdAsync(Guid userId, Guid id, CancellationToken ct = default);
        Task<PaginatedResult<Activity>> ListAsync(ActivityFilter filter, CancellationToken ct = default);
        Task AddAsync(Activity activity, CancellationToken ct = default);
        Task UpdateAsync(Activity activity, CancellationToken ct = default);
        Task DeleteAsync(Guid userId, Guid id, CancellationToken ct = default);

        Task<(int totalMin, double? totalKm, int? totalCalories)> GetDailySummaryAsync(Guid userId, DateTime dateUtc, CancellationToken ct = default);
        Task<IReadOnlyList<DailyAggregateDto>> GetRangeSummaryAsync(Guid userId, DateTime fromUtc, DateTime toUtc, CancellationToken ct = default);
    }

    public sealed record DailyAggregateDto(DateTime DateUtc, int TotalMin, double? TotalKm, int? TotalCalories);
}
