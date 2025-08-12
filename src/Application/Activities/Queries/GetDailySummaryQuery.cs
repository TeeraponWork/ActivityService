using Application.Abstractions;
using MediatR;

namespace Application.Activities.Queries
{
    public sealed record GetDailySummaryQuery(DateTime DateUtc) : IRequest<(int totalMin, double? totalKm, int? totalCalories)>;

    public sealed class GetDailySummaryHandler : IRequestHandler<GetDailySummaryQuery, (int, double?, int?)>
    {
        private readonly IUserContext _user;
        private readonly IActivityRepository _repo;

        public GetDailySummaryHandler(IUserContext user, IActivityRepository repo)
        { _user = user; _repo = repo; }

        public Task<(int, double?, int?)> Handle(GetDailySummaryQuery q, CancellationToken ct)
            => _repo.GetDailySummaryAsync(_user.UserId, q.DateUtc, ct);
    }
}
