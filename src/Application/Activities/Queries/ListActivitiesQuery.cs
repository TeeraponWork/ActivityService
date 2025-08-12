using Application.Abstractions;
using Application.Common;
using Domain.Entities;
using Domain.Enums;
using MediatR;

namespace Application.Activities.Queries
{
    public sealed record ListActivitiesQuery(
    DateTime? DateFromUtc, DateTime? DateToUtc, ActivityType[]? Types, int Page = 1, int PageSize = 20
) : IRequest<PaginatedResult<Activity>>;

    public sealed class ListActivitiesHandler : IRequestHandler<ListActivitiesQuery, PaginatedResult<Activity>>
    {
        private readonly IUserContext _user;
        private readonly IActivityRepository _repo;

        public ListActivitiesHandler(IUserContext user, IActivityRepository repo)
        { _user = user; _repo = repo; }

        public Task<PaginatedResult<Activity>> Handle(ListActivitiesQuery q, CancellationToken ct)
        {
            var filter = new ActivityFilter(_user.UserId, q.DateFromUtc, q.DateToUtc, q.Types, q.Page, q.PageSize);
            return _repo.ListAsync(filter, ct);
        }
    }
}
