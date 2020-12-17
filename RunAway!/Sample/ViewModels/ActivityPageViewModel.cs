using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using Prism.Navigation;

using Sample.Domain;
using Sample.Localization;

using Sharpnado.Presentation.Forms.ViewModels;

using SkiaSharpnado.Maps.Presentation.ViewModels.SessionMap;
using SkiaSharpnado.ViewModels;

using Xamarin.Forms;

namespace Sample.ViewModels
{
    public class ActivityPageViewModel : ViewModelBase
    {
        private readonly ITcxActivityService _activityService;

        private TimeSpan _currentTime;
        private string _currentHeartRate;
        private string _currentSpeed;
        private string _currentAltitude;
        private string _currentDistance;

        public ActivityPageViewModel(INavigationService navigationService, ITcxActivityService activityService)
            : base(navigationService)
        {
            _activityService = activityService;

            //Loader = new ViewModelLoader<SessionMapInfo>(emptyStateMessage: AppResources.EmptyActivityMessage);
        }

        //public ViewModelLoader<SessionMapInfo> Loader { get; }

        public SessionGraphInfo GraphInfo { get; private set; }

        public ActivityHeaderViewModel Header { get; private set; }

        public TimeSpan CurrentTime
        {
            get => _currentTime;
            set
            {
                SetProperty(ref _currentTime, value);
                OnCurrentTimeChanged();
            }
        }

        public string CurrentHeartRate
        {
            get => _currentHeartRate;
            set => SetProperty(ref _currentHeartRate, value);
        }

        public string CurrentSpeed
        {
            get => _currentSpeed;
            set => SetProperty(ref _currentSpeed, value);
        }

        public string CurrentAltitude
        {
            get => _currentAltitude;
            set => SetProperty(ref _currentAltitude, value);
        }

        public string CurrentDistance
        {
            get => _currentDistance;
            set => SetProperty(ref _currentDistance, value);
        }

        public override void OnNavigatedTo(NavigationParameters parameters)
        {
            string activityId = parameters.GetValue<string>("activityId");

            var date = DateTime.ParseExact(activityId, "yyyyMMdd_HHmm", CultureInfo.InvariantCulture);
            Title = date.ToLongDateString();

            //Loader.Load(() => LoadAsync(activityId));
        }

        private void OnCurrentTimeChanged()
        {
            if (GraphInfo == null)
            {
                return;
            }

            var currentPoint = GraphInfo.SessionPoints.First(p => p.Time >= CurrentTime);

            CurrentHeartRate = currentPoint.HeartRate?.ToString() ?? AppResources.NoValue;
            CurrentSpeed = currentPoint.Speed?.ToString("0.0") ?? AppResources.NoValue;
            CurrentAltitude = currentPoint.Altitude?.ToString() ?? AppResources.NoValue;
            CurrentDistance = currentPoint.Distance != null
                ? (currentPoint.Distance.Value / 1000f).ToString("0.0")
                : AppResources.NoValue;
        }

        private async Task<SessionMapInfo> LoadAsync(string activityId)
        {
            var activity = await _activityService.GetActivityAsync(activityId);

            if (activity.Lap[0].Track.Count < 2)
            {
                return null;
            }

            var activityPoints = activity.ToActivityPoints();
            var activityHeader = activity.ToActivityHeader();
            Header = new ActivityHeaderViewModel(activityHeader, new List<IDispersionSpan>());
            RaisePropertyChanged(nameof(Header));

            double maxSpeed = activity.Lap[0].MaximumSpeed * 3.6f;
            Color? SelectColorBySpeed(ISessionDisplayablePoint point)
            {
                if (point.Speed == null)
                {
                    return null;
                }

                return HumanEffortComputer.BySpeed.GetColor(point.Speed, maxSpeed);
            }

            Color? SelectColorByHeartRate(ISessionDisplayablePoint point)
            {
                if (point.HeartRate == null)
                {
                    return null;
                }

                return HumanEffortComputer.ByHeartBeat.GetColor(point.HeartRate);
            }

            int markerInterval = 100;
            int distanceInternal = 100;
            int totalDistance = activityHeader.DistanceInMeters;
            if (totalDistance >= 100000)
            {
                markerInterval = 5000;
                distanceInternal = 10000;
            }
            else if (totalDistance >= 50000)
            {
                markerInterval = 2000;
                distanceInternal = 5000;
            }
            else if (totalDistance >= 10000)
            {
                markerInterval = 1000;
                distanceInternal = 2000;
            }
            else if (totalDistance >= 5000)
            {
                markerInterval = 500;
                distanceInternal = 1000;
            }

            SessionMapInfo mapInfo;
            if (Header.AverageHeartRate.HasValue)
            {
                mapInfo = SessionMapInfo.Create(
                    activityPoints,
                    SelectColorByHeartRate,
                    markerInterval,
                    distanceInternal);
            }
            else
            {
                mapInfo = SessionMapInfo.Create(
                    activityPoints,
                    SelectColorBySpeed,
                    1000,
                    1000);
            }

            GraphInfo = SessionGraphInfo.CreateSessionGraphInfo(mapInfo.SessionPoints);
            RaisePropertyChanged(nameof(GraphInfo));

            return mapInfo;
        }
    }
}