using System;
using System.Collections.Generic;
using System.Linq;

using SkiaSharpnado.Maps.Domain;

using TcxTools;

namespace Sample.Domain
{
    public static class TcxActivityConverter
    {
        public static ActivityHeader ToActivityHeader(this Activity activity)
        {
            var lap = activity.Lap[0];
            return new ActivityHeader(
                lap.Track.Last().Time,
                TimeSpan.FromSeconds(lap.TotalTimeSeconds),
                (int)lap.DistanceMeters,
                lap.Calories,
                lap.AverageHeartRateBpm?.Value,
                lap.MaximumHeartRateBpm?.Value,
                lap.MaximumSpeed);
        }

        public static List<ActivityPoint> ToActivityPoints(this Activity activity)
        {
            var result = new List<ActivityPoint>();
            var track = activity.Lap[0].Track;
            foreach (var point in track)
            {
                result.Add(
                    new ActivityPoint(
                        point.Time,
                        point.HeartRateBpm?.Value,
                        point.Position == null
                            ? LatLong.Empty
                            : new LatLong(point.Position.LatitudeDegrees, point.Position.LongitudeDegrees),
                        (int)point.DistanceMeters,
                        (int)point.AltitudeMeters));
            }

            return result;
        }
    }
}
