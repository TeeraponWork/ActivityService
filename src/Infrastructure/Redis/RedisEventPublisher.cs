using System.Text.Json;
using Application.Abstractions;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace Infrastructure.Redis
{
    public sealed class RedisEventPublisher : IEventPublisher, IAsyncDisposable
    {
        private readonly ISubscriber _bus;
        private readonly string _channel;
        private readonly ConnectionMultiplexer _conn;

        public RedisEventPublisher(IOptions<RedisSettings> options)
        {
            _conn = ConnectionMultiplexer.Connect(options.Value.ConnectionString);
            _bus = _conn.GetSubscriber();
            _channel = options.Value.EventsChannel;
        }

        public Task PublishAsync(string eventName, object payload, CancellationToken ct = default)
        {
            var envelope = new { type = eventName, occurredAtUtc = DateTime.UtcNow, data = payload };
            var json = JsonSerializer.Serialize(envelope);
            return _bus.PublishAsync(_channel, json);
        }

        public async ValueTask DisposeAsync()
        {
            await _conn.CloseAsync();
            _conn.Dispose();
        }
    }
}
