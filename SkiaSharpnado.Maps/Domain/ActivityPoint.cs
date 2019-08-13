using System;

namespace SkiaSharpnado.Maps.Domain
{
    public class ActivityPoint
    {
        public ActivityPoint(DateTime timeStamp, int? heartRate, LatLong position, int distanceInMeters, int altitudeInMeters, double? speed = null)
        {
            TimeStamp = timeStamp;
            HeartRate = heartRate;
            Position = position;
            DistanceInMeters = distanceInMeters;
            AltitudeInMeters = altitudeInMeters;
            Speed = speed;
        }

        public DateTime TimeStamp { get; }

        public int? HeartRate { get; }

        public LatLong Position { get; }

        public int DistanceInMeters { get; }

        public int AltitudeInMeters { get; }

        public double? Speed { get; }
    }
}
