using Domain.Enums;

namespace Api.Dtos
{
    public sealed record CreateActivityRequest(
    ActivityType Type,
    DateTime StartAtUtc,
    int DurationMin,
    double? DistanceKm,
    int? Steps,
    int? Calories,
    int? PerceivedExertion,
    string? Notes
);
}
