using System;

namespace SkiaSharpnado.Maps.Domain
{
    public class ActivityHeader
    {
        public ActivityHeader(
            DateTime lastPointTime,
            TimeSpan duration,
            int distanceInMeters,
            int caloriesBurnt,
            int? averageHeartRate,
            int? maximumHeartRate,
            double maximumSpeed)
        {
            LastPointTime = lastPointTime;
            Duration = duration;
            DistanceInMeters = distanceInMeters;
            CaloriesBurnt = caloriesBurnt;
            AverageHeartRate = averageHeartRate;
            MaximumHeartRate = maximumHeartRate;
            MaximumSpeed = maximumSpeed;
        }

        public string Id => LastPointTime.ToString("yyyyMMdd_HHmm");

        public DateTime LastPointTime { get; }

        public TimeSpan Duration { get;  }

        public int DistanceInMeters { get; }

        public int CaloriesBurnt { get; }

        public int? AverageHeartRate { get; }

        public int? MaximumHeartRate { get; }

        public double MaximumSpeed { get; }
    }
}
