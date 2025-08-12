using Application.Abstractions;
using HealthChecks.MongoDb;
using Infrastructure.Mongo;
using Infrastructure.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<MongoDbSettings>(config.GetSection("Mongo"));
            services.AddSingleton<MongoContext>();
            services.AddSingleton<IActivityRepository, ActivityRepository>();

            services.Configure<RedisSettings>(config.GetSection("Redis"));
            services.AddSingleton<IEventPublisher, RedisEventPublisher>();

            services.AddHealthChecks().AddMongoDb(sp => sp.GetRequiredService<IMongoClient>(), name: "mongodb");


            return services;
        }
    }
}
