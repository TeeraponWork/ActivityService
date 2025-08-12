using Domain.Enums;

namespace Api.Dtos
{
    public sealed record UpdateActivityRequest(
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
