using Application.Abstractions;
using MediatR;

namespace Application.Activities.Commands
{
    public sealed record DeleteActivityCommand(Guid Id) : IRequest;

    public sealed class DeleteActivityHandler : IRequestHandler<DeleteActivityCommand>
    {
        private readonly IUserContext _user;
        private readonly IActivityRepository _repo;
        private readonly IEventPublisher _events;

        public DeleteActivityHandler(IUserContext user, IActivityRepository repo, IEventPublisher events)
        { _user = user; _repo = repo; _events = events; }

        public async Task Handle(DeleteActivityCommand cmd, CancellationToken ct)
        {
            await _repo.DeleteAsync(_user.UserId, cmd.Id, ct);
            await _events.PublishAsync("activity.deleted", new { Id = cmd.Id, UserId = _user.UserId }, ct);
        }
    }

}
