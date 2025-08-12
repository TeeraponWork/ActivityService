namespace Infrastructure.Redis
{
    public sealed class RedisSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string EventsChannel { get; set; } = "events.activity";
    }
}
