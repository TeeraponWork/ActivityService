using Api.Dtos;
using Application.Abstractions;
using Application.Activities.Commands;
using Application.Activities.Queries;
using Application.Common;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ActivitiesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ActivitiesController(IMediator mediator) => _mediator = mediator;

    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromBody] CreateActivityRequest req, CancellationToken ct)
    {
        var id = await _mediator.Send(new CreateActivityCommand(req.Type, req.StartAtUtc, req.DurationMin,
            req.DistanceKm, req.Steps, req.Calories, req.PerceivedExertion, req.Notes), ct);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpGet("{id}")]
    public Task<Activity?> GetById(Guid id, CancellationToken ct)
        => _mediator.Send(new GetActivityByIdQuery(id), ct);

    [HttpGet]
    public Task<PaginatedResult<Activity>> List([FromQuery] ListActivitiesQueryParams q, CancellationToken ct)
        => _mediator.Send(new ListActivitiesQuery(q.DateFromUtc, q.DateToUtc, q.Types, q.Page, q.PageSize), ct);

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateActivityRequest req, CancellationToken ct)
    {
        await _mediator.Send(new UpdateActivityCommand(id, req.Type, req.StartAtUtc, req.DurationMin,
            req.DistanceKm, req.Steps, req.Calories, req.PerceivedExertion, req.Notes), ct);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _mediator.Send(new DeleteActivityCommand(id), ct);
        return NoContent();
    }

    [HttpGet("summary/daily")]
    public Task<(int totalMin, double? totalKm, int? totalCalories)> Daily([FromQuery] DateTime dateUtc, CancellationToken ct)
        => _mediator.Send(new GetDailySummaryQuery(dateUtc), ct);

    [HttpGet("summary/range")]
    public Task<IReadOnlyList<DailyAggregateDto>> Range([FromQuery] DateTime fromUtc, [FromQuery] DateTime toUtc, CancellationToken ct)
        => _mediator.Send(new GetRangeSummaryQuery(fromUtc, toUtc), ct);
}
