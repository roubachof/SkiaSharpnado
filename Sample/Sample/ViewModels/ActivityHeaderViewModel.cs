using System;
using System.Collections.Generic;

using Sample.Localization;

using SkiaSharpnado.Maps.Domain;
using SkiaSharpnado.ViewModels;

namespace Sample.ViewModels
{
    public class ActivityHeaderViewModel
    {
        public ActivityHeaderViewModel(ActivityHeader header, List<IDispersionSpan> dispersion)
        {
            AthleteName = header.AthleteName;
            Sport = header.Sport;
            Dispersion = dispersion;
            LastPointTime = header.LastPointTime;
            Duration = header.Duration;
            DistanceInMeters = header.DistanceInMeters;
            CaloriesBurnt = header.CaloriesBurnt;
            AverageHeartRate = header.AverageHeartRate;
            MaximumHeartRate = header.MaximumHeartRate;
            MaximumSpeed = header.MaximumSpeed;

            DisplayableCaloriesBurnt = CaloriesBurnt == 0 ? AppResources.NoValue : CaloriesBurnt.ToString();
            DisplayableStartTime = LastPointTime.ToLocalTime().ToSmartShortDate();
            DisplayableDistance = (DistanceInMeters / 1000f).ToString("0.00");
            DisplayableTimeSpan = $"{Duration:h\\:mm}";
            DisplayableAverageSpeed = $"{((DistanceInMeters / 1000f) / Duration.TotalHours):0.0}";
            DisplayableAverageHeartRate =
                AverageHeartRate == null ? AppResources.NoValue : AverageHeartRate.Value.ToString();
        }

        public string AthleteName { get; }

        public Sport Sport { get; }

        public string Id => LastPointTime.ToString("yyyyMMdd_HHmm");

        public List<IDispersionSpan> Dispersion { get; }

        public DateTime LastPointTime { get; }

        public string DisplayableStartTime { get; }

        public TimeSpan Duration { get;  }

        public int DistanceInMeters { get; }

        public string DisplayableDistance { get; }

        public string DisplayableTimeSpan { get; }

        public int CaloriesBurnt { get; }

        public string DisplayableCaloriesBurnt { get; }

        public int? AverageHeartRate { get; }

        public string DisplayableAverageHeartRate { get; }

        public int? MaximumHeartRate { get; }

        public string DisplayableAverageSpeed { get; }

        public double MaximumSpeed { get; }
    }
}