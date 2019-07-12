using System;

using SkiaSharpnado.Maps.Domain;

using Xamarin.Forms;

namespace SkiaSharpnado.Maps.Presentation.ViewModels.SessionMap
{
    public interface ISessionDisplayablePoint
    {
        TimeSpan Time { get; }

        Color MapPointColor { get; }

        int? Altitude { get; }

        int? HeartRate { get; }

        double? Speed { get; }

        LatLong Position { get; }

        bool HasMarker { get; }

        string Label { get; }

        int? Distance { get; }
    }

    public class SessionDisplayablePoint : ISessionDisplayablePoint
    {
        public SessionDisplayablePoint(
            TimeSpan timeSpan,
            int? heartRate,
            int? distance,
            int? altitude,
            double? speed,
            LatLong position,
            bool hasMarker = false,
            string label = null)
        {
            Time = timeSpan;

            Altitude = altitude;
            HeartRate = heartRate;
            Speed = speed;
            Position = position;
            Distance = distance;

            MapPointColor = Color.Default;
            HasMarker = hasMarker;
            Label = label;
        }

        public TimeSpan Time { get; }

        public Color MapPointColor { get; private set; }

        public int? Altitude { get; }

        public int? Distance { get; }

        public int? HeartRate { get; }

        public LatLong Position { get; }

        public bool HasMarker { get; }

        public string Label { get; }

        public double? Speed { get; }

        public bool HasPosition => Position != LatLong.Empty;

        public void SetPointColor(Color color)
        {
            MapPointColor = color;
        }
    }
}
