using Application.Abstractions;
using MediatR;

namespace Application.Activities.Queries
{
    public sealed record GetRangeSummaryQuery(DateTime FromUtc, DateTime ToUtc) : IRequest<IReadOnlyList<DailyAggregateDto>>;

    public sealed class GetRangeSummaryHandler : IRequestHandler<GetRangeSummaryQuery, IReadOnlyList<DailyAggregateDto>>
    {
        private readonly IUserContext _user;
        private readonly IActivityRepository _repo;

        public GetRangeSummaryHandler(IUserContext user, IActivityRepository repo)
        { _user = user; _repo = repo; }

        public Task<IReadOnlyList<DailyAggregateDto>> Handle(GetRangeSummaryQuery q, CancellationToken ct)
            => _repo.GetRangeSummaryAsync(_user.UserId, q.FromUtc, q.ToUtc, ct);
    }
}
