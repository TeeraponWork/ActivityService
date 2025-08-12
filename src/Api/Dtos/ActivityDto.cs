using Domain.Enums;

namespace Api.Dtos
{
    public sealed record ActivityDto(
    Guid Id,
    ActivityType Type,
    DateTime StartAtUtc,
    int DurationMin,
    double? DistanceKm,
    int? Steps,
    int? Calories,
    int? PerceivedExertion,
    string? Notes,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc
);
}
