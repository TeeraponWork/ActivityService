using Domain.Enums;

namespace Domain.Entities
{
    public sealed class Activity
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public ActivityType Type { get; private set; }
        public DateTime StartAtUtc { get; private set; }
        public int DurationMin { get; private set; } // นาที
        public double? DistanceKm { get; private set; }
        public int? Steps { get; private set; }
        public int? Calories { get; private set; }
        public int? PerceivedExertion { get; private set; } // RPE 1-10
        public string? Notes { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime UpdatedAtUtc { get; private set; }

        private Activity() { }

        private Activity(
            Guid id, Guid userId, ActivityType type,
            DateTime startAtUtc, int durationMin,
            double? distanceKm, int? steps, int? calories, int? rpe, string? notes)
        {
            Id = id == Guid.Empty ? Guid.NewGuid() : id;
            UserId = userId;
            Type = type;
            StartAtUtc = DateTime.SpecifyKind(startAtUtc, DateTimeKind.Utc);
            DurationMin = durationMin;
            DistanceKm = distanceKm;
            Steps = steps;
            Calories = calories;
            PerceivedExertion = rpe;
            Notes = notes;
            CreatedAtUtc = DateTime.UtcNow;
            UpdatedAtUtc = CreatedAtUtc;
            Validate();
        }

        public static Activity Create(
            Guid userId, ActivityType type,
            DateTime startAtUtc, int durationMin,
            double? distanceKm, int? steps, int? calories, int? rpe, string? notes)
            => new Activity(Guid.NewGuid(), userId, type, startAtUtc, durationMin, distanceKm, steps, calories, rpe, notes);

        public void Update(
            ActivityType type, DateTime startAtUtc, int durationMin,
            double? distanceKm, int? steps, int? calories, int? rpe, string? notes)
        {
            Type = type;
            StartAtUtc = DateTime.SpecifyKind(startAtUtc, DateTimeKind.Utc);
            DurationMin = durationMin;
            DistanceKm = distanceKm;
            Steps = steps;
            Calories = calories;
            PerceivedExertion = rpe;
            Notes = notes;
            UpdatedAtUtc = DateTime.UtcNow;
            Validate();
        }

        private void Validate()
        {
            if (UserId == Guid.Empty) throw new ArgumentException("UserId is required");
            if (DurationMin <= 0) throw new ArgumentException("Duration must be > 0 minute");
            if (PerceivedExertion is < 1 or > 10) PerceivedExertion = null; // normalize
            if (DistanceKm is < 0) DistanceKm = null;
            if (Steps is < 0) Steps = null;
            if (Calories is < 0) Calories = null;
        }
    }
}
