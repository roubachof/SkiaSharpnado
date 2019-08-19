using System;
using System.Collections.Generic;

using SkiaSharpnado.Maps.Domain;

using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace SkiaSharpnado.Maps.Presentation.ViewModels.SessionMap
{
    public class SessionMapInfo
    {
        public SessionMapInfo(
            IReadOnlyList<SessionDisplayablePoint> sessionPoints,
            Position bottomLeft,
            Position topRight,
            int totalDurationInSeconds)
        {
            SessionPoints = sessionPoints;
            BottomLeft = bottomLeft;
            TopRight = topRight;
            Region = GeoCalculation.BoundsToMapSpan(bottomLeft, topRight);
            TotalDurationInSeconds = totalDurationInSeconds;
        }

        public static SessionMapInfo Create(
            IReadOnlyList<ActivityPoint> points,
            Func<ISessionDisplayablePoint, Color?> colorBaseValueSelector,
            int markerInterval = int.MaxValue,
            int displayDistanceInterval = int.MaxValue)
        {
            if (points == null || points.Count < 2)
            {
                throw new ArgumentException();
            }

            var sessionPoints = new SessionDisplayablePoint[points.Count];

            double topLatitude = LatLong.Min.Latitude;
            double bottomLatitude = LatLong.Max.Latitude;
            double leftLongitude = LatLong.Max.Longitude;
            double rightLongitude = LatLong.Min.Longitude;

            int nextMarkerDistance = markerInterval;
            int nextDisplayDistance = displayDistanceInterval;

            SessionDisplayablePoint previousPoint = null;
            DateTime startTime = points[0].TimeStamp;
            for (int index = 0; index < points.Count; index++)
            {
                ActivityPoint point = points[index];

                if (point.Position != LatLong.Empty)
                {
                    topLatitude = Math.Max(point.Position.Latitude, topLatitude);
                    bottomLatitude = Math.Min(point.Position.Latitude, bottomLatitude);
                    leftLongitude = Math.Min(point.Position.Longitude, leftLongitude);
                    rightLongitude = Math.Max(point.Position.Longitude, rightLongitude);
                }

                bool hasMarker = false;
                if (point.DistanceInMeters > nextMarkerDistance)
                {
                    hasMarker = true;
                    nextMarkerDistance += markerInterval;
                }

                string displayDistance = null;
                if (point.DistanceInMeters > nextDisplayDistance)
                {
                    displayDistance = nextDisplayDistance.ToString();
                    nextDisplayDistance += displayDistanceInterval;
                }

                TimeSpan elapsedTime = point.TimeStamp - startTime;

                double? speed = point.Speed;
                if (speed == null 
                    && previousPoint != null 
                    && previousPoint.HasPosition 
                    && previousPoint.Distance.HasValue
                    && point.Position != LatLong.Empty 
                    && point.DistanceInMeters > 0 
                    && elapsedTime.TotalSeconds > 0)
                {
                    double kilometersTraveled =
                        GeoCalculation.HaversineDistance(previousPoint.Position, point.Position);
                    double hoursElapsed = (elapsedTime - previousPoint.Time).TotalHours;
                    speed = kilometersTraveled / hoursElapsed;
                }

                var currentPoint = sessionPoints[index] = new SessionDisplayablePoint(
                    elapsedTime,
                    point.HeartRate,
                    point.DistanceInMeters,
                    point.AltitudeInMeters,
                    speed,
                    point.Position,
                    hasMarker,
                    displayDistance);

                Color mapPointColor =
                    colorBaseValueSelector(currentPoint) ?? previousPoint?.MapPointColor ?? Color.Gray;

                currentPoint.SetPointColor(mapPointColor);
                previousPoint = currentPoint;
            }

            return new SessionMapInfo(
                sessionPoints,
                new Position(bottomLatitude, leftLongitude),
                new Position(topLatitude, rightLongitude),
                previousPoint != null ? (int) previousPoint.Time.TotalSeconds : 0);
        }

        public IReadOnlyList<SessionDisplayablePoint> SessionPoints { get; }

        public MapSpan Region { get; }

        public Position BottomLeft { get; }

        public Position TopRight { get; }

        public int TotalDurationInSeconds { get; }
    }
}
