using Domain.Entities;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;

namespace Infrastructure.Mongo
{
    public sealed class MongoContext
    {
        public IMongoDatabase Database { get; }
        public IMongoCollection<Activity> Activities { get; }

        static bool _mapped;

        public MongoContext(IOptions<MongoDbSettings> options)
        {
            // register conventions & class map ครั้งเดียว
            if (!_mapped)
            {
                var pack = new ConventionPack
            {
                new CamelCaseElementNameConvention(),
                new IgnoreExtraElementsConvention(true)
            };
                ConventionRegistry.Register("activity_conventions", pack, _ => true);

                if (!BsonClassMap.IsClassMapRegistered(typeof(Activity)))
                {
                    BsonClassMap.RegisterClassMap<Activity>(cm =>
                    {
                        cm.AutoMap();

                        // กำหนด Guid ให้ชัดว่าใช้ Standard (Binary subtype 4)
                        cm.MapIdProperty(x => x.Id)
                          .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
                        cm.MapMember(x => x.UserId)
                          .SetSerializer(new GuidSerializer(GuidRepresentation.Standard));
                    });
                }

                _mapped = true;
            }

            var cfg = options.Value;
            var client = new MongoClient(cfg.ConnectionString);
            Database = client.GetDatabase(cfg.Database);
            Activities = Database.GetCollection<Activity>(cfg.ActivitiesCollection);
        }
    }
}
