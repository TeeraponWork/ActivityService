using Application.Abstractions;
using Domain.Entities;
using MediatR;

namespace Application.Activities.Queries
{
    public sealed record GetActivityByIdQuery(Guid Id) : IRequest<Activity?>;

    public sealed class GetActivityByIdHandler : IRequestHandler<GetActivityByIdQuery, Activity?>
    {
        private readonly IUserContext _user;
        private readonly IActivityRepository _repo;

        public GetActivityByIdHandler(IUserContext user, IActivityRepository repo)
        { _user = user; _repo = repo; }

        public Task<Activity?> Handle(GetActivityByIdQuery q, CancellationToken ct)
            => _repo.GetByIdAsync(_user.UserId, q.Id, ct);
    }
}
