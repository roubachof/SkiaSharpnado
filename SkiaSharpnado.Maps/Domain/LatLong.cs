using System;

namespace SkiaSharpnado.Maps.Domain
{
    public struct LatLong
    {
        public static readonly LatLong Empty = new LatLong(double.NaN, double.NaN);
        public static readonly LatLong Min = new LatLong(-90, -180);
        public static readonly LatLong Max = new LatLong(90, 180);

        public LatLong(double latitude, double longitude)
        {
            Latitude = Math.Min(Math.Max(latitude, -90.0), 90.0);
            Longitude = Math.Min(Math.Max(longitude, -180.0), 180.0);
        }

        public double Latitude { get; }

        public double Longitude { get; }

        public static bool operator ==(LatLong left, LatLong right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(LatLong left, LatLong right)
        {
            return !Equals(left, right);
        }

        public static LatLong operator -(LatLong left, LatLong right)
        {
            return new LatLong(left.Latitude - right.Latitude, left.Longitude - right.Longitude);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is LatLong))
            {
                return false;
            }

            var other = (LatLong)obj;
            const double tolerance = 0.00000001;
            return Math.Abs(Latitude - other.Latitude) < tolerance && Math.Abs(Longitude - other.Longitude) < tolerance;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Latitude.GetHashCode();
                hashCode = (hashCode * 397) ^ Longitude.GetHashCode();
                return hashCode;
            }
        }
    }
}
