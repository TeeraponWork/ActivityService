using Application.Abstractions;
using Application.Common;
using Domain.Entities;
using MongoDB.Driver;
using MongoDB.Bson;
using System;
using System.Linq;

namespace Infrastructure.Mongo;

public sealed class ActivityRepository : IActivityRepository
{
    private readonly MongoContext _ctx;

    public ActivityRepository(MongoContext ctx) => _ctx = ctx;

    public async Task<Activity?> GetByIdAsync(Guid userId, Guid id, CancellationToken ct = default)
    {
        var filter = Builders<Activity>.Filter.Eq(x => x.Id, id) &
                     Builders<Activity>.Filter.Eq(x => x.UserId, userId);

        return await _ctx.Activities.Find(filter).FirstOrDefaultAsync(ct);
    }

    public async Task<PaginatedResult<Activity>> ListAsync(ActivityFilter f, CancellationToken ct = default)
    {
        var filter = Builders<Activity>.Filter.Eq(x => x.UserId, f.UserId);

        if (f.DateFromUtc.HasValue)
            filter &= Builders<Activity>.Filter.Gte(x => x.StartAtUtc, f.DateFromUtc.Value);
        if (f.DateToUtc.HasValue)
            filter &= Builders<Activity>.Filter.Lte(x => x.StartAtUtc, f.DateToUtc.Value);
        if (f.Types is { Length: > 0 })
            filter &= Builders<Activity>.Filter.In(x => x.Type, f.Types);

        var page = f.Page <= 0 ? 1 : f.Page;
        var size = f.PageSize <= 0 ? 20 : Math.Min(f.PageSize, 100);

        var query = _ctx.Activities.Find(filter);

        var total = (int)await query.CountDocumentsAsync(ct);
        var items = await query
            .SortByDescending(x => x.StartAtUtc)
            .Skip((page - 1) * size)
            .Limit(size)
            .ToListAsync(ct);

        return new PaginatedResult<Activity>(items, total, page, size);
    }

    public Task AddAsync(Activity activity, CancellationToken ct = default)
        => _ctx.Activities.InsertOneAsync(activity, cancellationToken: ct);

    public Task UpdateAsync(Activity activity, CancellationToken ct = default)
        => _ctx.Activities.ReplaceOneAsync(
            x => x.Id == activity.Id && x.UserId == activity.UserId,
            activity,
            cancellationToken: ct);

    public Task DeleteAsync(Guid userId, Guid id, CancellationToken ct = default)
        => _ctx.Activities.DeleteOneAsync(
            x => x.Id == id && x.UserId == userId,
            cancellationToken: ct);

    public async Task<(int totalMin, double? totalKm, int? totalCalories)> GetDailySummaryAsync(
        Guid userId, DateTime dateUtc, CancellationToken ct = default)
    {
        var start = dateUtc.Date;
        var end = start.AddDays(1).AddTicks(-1);

        var match = Builders<Activity>.Filter.Eq(x => x.UserId, userId) &
                    Builders<Activity>.Filter.Gte(x => x.StartAtUtc, start) &
                    Builders<Activity>.Filter.Lte(x => x.StartAtUtc, end);

        var proj = await _ctx.Activities.Aggregate()
            .Match(match)
            .Group(x => 1, g => new
            {
                TotalMin = g.Sum(a => a.DurationMin),
                TotalKm = g.Sum(a => a.DistanceKm ?? 0),
                TotalCalories = g.Sum(a => a.Calories ?? 0)
            })
            .FirstOrDefaultAsync(ct);

        if (proj is null) return (0, null, null);

        var km = proj.TotalKm <= 0 ? (double?)null : proj.TotalKm;
        var cal = proj.TotalCalories <= 0 ? (int?)null : proj.TotalCalories;
        return (proj.TotalMin, km, cal);
    }

    public async Task<IReadOnlyList<DailyAggregateDto>> GetRangeSummaryAsync(
        Guid userId, DateTime fromUtc, DateTime toUtc, CancellationToken ct = default)
    {
        var match = Builders<Activity>.Filter.Eq(x => x.UserId, userId) &
                    Builders<Activity>.Filter.Gte(x => x.StartAtUtc, fromUtc.Date) &
                    Builders<Activity>.Filter.Lte(x => x.StartAtUtc, toUtc.Date.AddDays(1).AddTicks(-1));

        var results = await _ctx.Activities.Aggregate()
            .Match(match)
            .AppendStage<BsonDocument>(
                new BsonDocument("$addFields",
                    new BsonDocument("day",
                        new BsonDocument("$dateTrunc",
                            new BsonDocument
                            {
                                { "date", "$startAtUtc" }, // camelCase field names
                                { "unit", "day" }
                            }))))
            .Group(new BsonDocument
            {
                { "_id", "$day" },
                { "totalMin", new BsonDocument("$sum", "$durationMin") },
                { "totalKm", new BsonDocument("$sum",
                    new BsonDocument("$ifNull", new BsonArray { "$distanceKm", 0.0 })) },
                { "totalCalories", new BsonDocument("$sum",
                    new BsonDocument("$ifNull", new BsonArray { "$calories", 0 })) }
            })
            .Sort(new BsonDocument("_id", 1))
            .ToListAsync(ct);

        var list = results.Select(r =>
        {
            var date = r["_id"].ToUniversalTime();

            var totalMin = r["totalMin"].IsInt32
                ? r["totalMin"].AsInt32
                : (int)r["totalMin"].AsInt64;

            var totalKmVal = r["totalKm"].IsDouble
                ? r["totalKm"].AsDouble
                : (r["totalKm"].IsInt32 ? r["totalKm"].AsInt32 : (double)r["totalKm"].AsInt64);

            var totalCaloriesVal = r["totalCalories"].IsInt32
                ? r["totalCalories"].AsInt32
                : (int)r["totalCalories"].AsInt64;

            double? km = totalKmVal <= 0 ? null : totalKmVal;
            int? calories = totalCaloriesVal <= 0 ? null : totalCaloriesVal;

            return new DailyAggregateDto(date, totalMin, km, calories);
        }).ToList();

        return list;
    }
}
