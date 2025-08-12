namespace Infrastructure.Mongo
{
    public sealed class MongoDbSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string Database { get; set; } = "activitydb";
        public string ActivitiesCollection { get; set; } = "activities";
    }
}
