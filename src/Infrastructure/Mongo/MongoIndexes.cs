using Domain.Entities;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Infrastructure.Mongo
{
    public static class MongoIndexes
    {
        public static async Task EnsureCollectionsAndIndexesAsync(MongoContext ctx, CancellationToken ct = default)
        {
            // 1) ensure collection 'activities'
            var collName = "activities";
            var existing = await ctx.Database.ListCollectionNames().ToListAsync(ct);
            if (!existing.Contains(collName))
            {
                await ctx.Database.CreateCollectionAsync(collName, cancellationToken: ct);
            }

            // 2) indexes for activities
            var idx1 = new CreateIndexModel<Activity>(
                Builders<Activity>.IndexKeys.Ascending(x => x.UserId).Descending(x => x.StartAtUtc));
            var idx2 = new CreateIndexModel<Activity>(
                Builders<Activity>.IndexKeys.Ascending(x => x.UserId).Ascending(x => x.Type).Descending(x => x.StartAtUtc));
            var idx3 = new CreateIndexModel<Activity>(
                Builders<Activity>.IndexKeys.Descending(x => x.CreatedAtUtc));

            await ctx.Activities.Indexes.CreateManyAsync(new[] { idx1, idx2, idx3 }, ct);

            // (ถ้าใช้สรุปรายวัน) ensure 'activity_daily'
            var dailyName = "activity_daily";
            if (!existing.Contains(dailyName))
                await ctx.Database.CreateCollectionAsync(dailyName, cancellationToken: ct);
            var daily = ctx.Database.GetCollection<BsonDocument>(dailyName);
            await daily.Indexes.CreateOneAsync(
                new CreateIndexModel<BsonDocument>(
                    Builders<BsonDocument>.IndexKeys
                        .Ascending("userId")
                        .Ascending("dateUtc"),
                    new CreateIndexOptions { Unique = true }
                ),
                cancellationToken: ct
            );

            // (ถ้าใช้ time-series สำหรับ sample) ensure 'activity_samples'
            // *ต้องเป็น MongoDB 5.0+*
            var samplesName = "activity_samples";
            var existings = await ctx.Database.ListCollectionNames().ToListAsync(ct);

            if (!existings.Contains(samplesName))
            {
                var options = new CreateCollectionOptions
                {
                    TimeSeriesOptions = new TimeSeriesOptions(timeField: "ts", metaField: "meta"),
                    // ตั้ง TTL ทั้งคอลเลกชัน = 180 วัน
                    ExpireAfter = TimeSpan.FromDays(180)
                };

                await ctx.Database.CreateCollectionAsync(samplesName, options, ct);

                // ไม่ต้องสร้าง TTL index เพิ่มแล้ว
            }
        }
    }
}
