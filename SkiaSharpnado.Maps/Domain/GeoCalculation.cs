using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharpnado.Maps.Presentation.Views;
using Xamarin.Forms.Maps;

namespace SkiaSharpnado.Maps.Domain
{
    /// <summary>
    ///     Tools for geo-calculation.
    /// </summary>
    public static class GeoCalculation
    {
        /// <summary>In kilometers.</summary>
        public const double EarthRadius = 6373d;

        public const double KilometersByDegree = 111.12219769899677d; // At 0°N (Equator)

        public static readonly double RadianToDegree = 180d / Math.PI;

        public static readonly double DegreeToRadian = Math.PI / 180d;

        /// <summary>
        ///     Squares the distance.
        ///     Warning: this method is poorly accurate.
        /// </summary>
        /// <param name="latitude1">The latitude1.</param>
        /// <param name="longitude1">The longitude1.</param>
        /// <param name="latitude2">The latitude2.</param>
        /// <param name="longitude2">The longitude2.</param>
        /// <returns>The square distance in degree.</returns>
        public static double SquareDistance(double latitude1, double longitude1, double latitude2, double longitude2)
        {
            // Ajust calculation on mid latitude point projection
            const double r = Math.PI / 360;
            double delta;
            return (delta = latitude1 - latitude2) * delta
                + (delta = longitude1 - longitude2)
                * delta
                * Math.Cos(
                    ((latitude1 < 0 ? -latitude1 : latitude1) % 90 + (latitude2 < 0 ? -latitude2 : latitude2) % 90)
                    * r);
        }

        /// <summary>
        ///     Calculate distance (in kilometers) between two points specified by latitude/longitude using Haversine formula.
        ///     http://en.wikipedia.org/wiki/Haversine_formula
        /// </summary>
        /// <param name="latitudePointA">The latitude point A.</param>
        /// <param name="longitudePointA">The longitude point A.</param>
        /// <param name="latitudePointB">The latitude point B.</param>
        /// <param name="longitudePointB">The longitude point B.</param>
        /// <returns>The distance in kilometers.</returns>
        public static double HaversineDistance(
            double latitudePointA,
            double longitudePointA,
            double latitudePointB,
            double longitudePointB)
        {
            double radianLatitudeA = latitudePointA * DegreeToRadian;
            double radianLatitudeB = latitudePointB * DegreeToRadian;

            double deltaLat = radianLatitudeB - radianLatitudeA;
            double deltaLong = longitudePointB * DegreeToRadian - longitudePointA * DegreeToRadian;
            double sinDeltaLatCoeff = Math.Sin(deltaLat / 2);
            double sinDeltaLongCoeff = Math.Sin(deltaLong / 2);
            double a = sinDeltaLatCoeff * sinDeltaLatCoeff
                + Math.Cos(radianLatitudeA) * Math.Cos(radianLatitudeB) * sinDeltaLongCoeff * sinDeltaLongCoeff;
            return 2 * EarthRadius * Math.Asin(Math.Min(1, Math.Sqrt(a)));
        }

        /// <summary>
        ///     Compute the Haversine distance.
        /// </summary>
        /// <param name="pointA">The point A.</param>
        /// <param name="pointB">The point B.</param>
        /// <returns>The distance in kilometers.</returns>
        public static double HaversineDistance(LatLong pointA, LatLong pointB)
        {
            return HaversineDistance(pointA.Latitude, pointA.Longitude, pointB.Latitude, pointB.Longitude);
        }

        public static double AdjustPointToPointDistance(double distance, int percentRatio)
        {
            return distance * percentRatio / 100d;
        }

        /// <summary>
        ///     Squares the distance.
        ///     Warning: this method is poorly accurate.
        /// </summary>
        /// <param name="point1">The point1.</param>
        /// <param name="point2">The point2.</param>
        /// <returns></returns>
        public static double SquareDistance(LatLong point1, LatLong point2)
        {
            return SquareDistance(point1.Latitude, point1.Longitude, point2.Latitude, point2.Longitude);
        }

        /// <summary>
        ///     Gets the distance (in kilometers) between two spherical points (in degrees).
        ///     Warning: this method is poorly accurate.
        /// </summary>
        public static double Distance(LatLong point1, LatLong point2)
        {
            return DegreeToKilometer(Math.Sqrt(SquareDistance(point1, point2)));
        }

        /// <summary>
        ///     Gets the distance (in kilometers) between two spherical points (in degrees).
        ///     Warning: this method is poorly accurate.
        /// </summary>
        public static double Distance(double latitude1, double longitude1, double latitude2, double longitude2)
        {
            return DegreeToKilometer(Math.Sqrt(SquareDistance(latitude1, longitude1, latitude2, longitude2)));
        }

        /// <summary>
        ///     Converts degrees to kilometer.
        /// </summary>
        /// <param name="degree">The degree.</param>
        /// <returns></returns>
        public static double DegreeToKilometer(double degree)
        {
            return degree * KilometersByDegree;
        }

        /// <summary>
        ///     Computes the angle defined by aBc using Al-Kashi.
        ///     Warning: this method is not accurate for triangles in spheric environnement.
        /// </summary>
        /// <param name="point1">The point1.</param>
        /// <param name="point2">The point2.</param>
        /// <param name="point3">The point3.</param>
        /// <returns></returns>
        public static double ComputeAngle(LatLong a, LatLong b, LatLong c)
        {
            if (a == b || a == c || b == c)
            {
                return double.NaN;
            }

            // Al-kashi: alpha = arccos (ab² + bc² - ac² / (2ab * bc))
            double abSquare = SquareDistance(a, b);
            double bcSquare = SquareDistance(b, c);
            double acSquare = SquareDistance(a, c);
            return RadianToDegree
                * Math.Acos((abSquare + bcSquare - acSquare) / (2 * Math.Sqrt(abSquare) * Math.Sqrt(bcSquare)));
        }

        public static MapSpan BoundsToMapSpan(Position bottomLeft, Position topRight)
        {
            var center = new Position(
                (topRight.Latitude + bottomLeft.Latitude) / 2,
                (topRight.Longitude + bottomLeft.Longitude) / 2);

            var distance = HaversineDistance(bottomLeft.ToLatLong(), topRight.ToLatLong()) * 1000;

            return MapSpan.FromCenterAndRadius(center, new Distance(distance / 2));
        }
    }
}