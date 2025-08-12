using Application.Abstractions;
using Domain.Enums;
using MediatR;

namespace Application.Activities.Commands
{
    public sealed record UpdateActivityCommand(
        Guid Id,
        ActivityType Type,
        DateTime StartAtUtc,
        int DurationMin,
        double? DistanceKm,
        int? Steps,
        int? Calories,
        int? PerceivedExertion,
        string? Notes
    ) : IRequest;

    public sealed class UpdateActivityHandler : IRequestHandler<UpdateActivityCommand>
    {
        private readonly IUserContext _user;
        private readonly IActivityRepository _repo;
        private readonly IEventPublisher _events;

        public UpdateActivityHandler(IUserContext user, IActivityRepository repo, IEventPublisher events)
        { _user = user; _repo = repo; _events = events; }

        public async Task Handle(UpdateActivityCommand cmd, CancellationToken ct)
        {
            var current = await _repo.GetByIdAsync(_user.UserId, cmd.Id, ct)
                ?? throw new InvalidOperationException("Activity not found");

            current.Update(cmd.Type, cmd.StartAtUtc, cmd.DurationMin,
                           cmd.DistanceKm, cmd.Steps, cmd.Calories, cmd.PerceivedExertion, cmd.Notes);

            await _repo.UpdateAsync(current, ct);
            await _events.PublishAsync("activity.updated",
                new { current.Id, current.UserId, current.UpdatedAtUtc }, ct);
        }
    }
}
