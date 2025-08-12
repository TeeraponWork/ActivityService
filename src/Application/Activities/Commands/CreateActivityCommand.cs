using Application.Abstractions;
using Domain.Entities;
using Domain.Enums;
using MediatR;
namespace Application.Activities.Commands
{
    public sealed record CreateActivityCommand(
    ActivityType Type,
    DateTime StartAtUtc,
    int DurationMin,
    double? DistanceKm,
    int? Steps,
    int? Calories,
    int? PerceivedExertion,
    string? Notes
) : IRequest<Guid>;

    public sealed class CreateActivityHandler : IRequestHandler<CreateActivityCommand, Guid>
    {
        private readonly IUserContext _user;
        private readonly IActivityRepository _repo;
        private readonly IEventPublisher _events;

        public CreateActivityHandler(IUserContext user, IActivityRepository repo, IEventPublisher events)
        { _user = user; _repo = repo; _events = events; }

        public async Task<Guid> Handle(CreateActivityCommand cmd, CancellationToken ct)
        {
            var entity = Activity.Create(_user.UserId, cmd.Type, cmd.StartAtUtc, cmd.DurationMin,
                cmd.DistanceKm, cmd.Steps, cmd.Calories, cmd.PerceivedExertion, cmd.Notes);

            await _repo.AddAsync(entity, ct);

            await _events.PublishAsync("activity.created", new
            {
                entity.Id,
                entity.UserId,
                Type = entity.Type.ToString(),
                entity.StartAtUtc,
                entity.DurationMin,
                entity.DistanceKm,
                entity.Steps,
                entity.Calories,
                entity.PerceivedExertion,
                entity.Notes,
                entity.CreatedAtUtc
            }, ct);

            return entity.Id;
        }
    }
}
