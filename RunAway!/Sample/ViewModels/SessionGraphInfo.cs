using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharpnado.Maps.Domain;
using SkiaSharpnado.Maps.Presentation.ViewModels.SessionMap;

namespace Sample.ViewModels
{
    public struct ValueBounds
    {
        public ValueBounds(double min, double max)
        {
            Min = min;
            Max = max;
        }

        public double Min { get; }

        public double Max { get; }
    }

    public class SessionGraphInfo
    {
        public SessionGraphInfo(
            IReadOnlyList<ISessionDisplayablePoint> sessionPoints,
            ValueBounds heartRate,
            ValueBounds speed,
            ValueBounds altitude,
            int totalDurationInSeconds)
        {
            SessionPoints = sessionPoints;
            HeartRate = heartRate;
            Speed = speed;
            Altitude = altitude;
            TotalDurationInSeconds = totalDurationInSeconds;
        }

        public static SessionGraphInfo CreateSessionGraphInfo(IReadOnlyList<ISessionDisplayablePoint> points)
        {
            if (points == null || points.Count < 2)
            {
                throw new ArgumentException();
            }

            var heartRateBounds = new ValueBounds(points.Min(p => p.HeartRate ?? int.MaxValue), points.Max(p => p.HeartRate ?? 0));
            var speedBounds = new ValueBounds(points.Min(p => p.Speed ?? int.MaxValue), points.Max(p => p.Speed ?? 0));
            var altitudeBounds = new ValueBounds(points.Min(p => p.Altitude ?? int.MaxValue), points.Max(p => p.Altitude ?? 0));

            int totalDuration = (int)points.Last().Time.TotalSeconds;

            return new SessionGraphInfo(points, heartRateBounds, speedBounds, altitudeBounds, totalDuration);
        }

        public IReadOnlyList<ISessionDisplayablePoint> SessionPoints { get; }

        public ValueBounds HeartRate { get; }

        public ValueBounds Speed { get; }

        public ValueBounds Altitude { get; }

        public int TotalDurationInSeconds { get; }
    }
}