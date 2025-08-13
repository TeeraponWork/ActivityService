using Application.Abstractions;
using HealthChecks.MongoDb;
using Infrastructure.Mongo;
using Infrastructure.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
        {
            // Mongo options
            services.Configure<MongoDbSettings>(config.GetSection("Mongo"));

            // Register IMongoClient จากค่าใน options
            services.AddSingleton<IMongoClient>(sp =>
            {
                var opt = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
                return new MongoClient(opt.ConnectionString);
            });

            services.AddSingleton<MongoContext>();
            services.AddSingleton<IActivityRepository, ActivityRepository>();

            // Redis
            services.Configure<RedisSettings>(config.GetSection("Redis"));
            services.AddSingleton<IEventPublisher, RedisEventPublisher>();

            // HealthChecks: ใช้ factory ดึง IMongoClient จาก DI
            services.AddHealthChecks()
                    .AddMongoDb(sp => sp.GetRequiredService<IMongoClient>(), name: "mongodb");

            return services;
        }
    }
}
