using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

using Prism.Navigation;

using Sample.Domain;
using Sample.Localization;

using Sharpnado.Presentation.Forms.ViewModels;

using SkiaSharpnado.Maps.Domain;
using SkiaSharpnado.Maps.Presentation.ViewModels.SessionMap;
using SkiaSharpnado.ViewModels;

using Xamarin.Forms;

namespace Sample.ViewModels
{
    public class ActivityPageViewModel : ViewModelBase
    {
        private readonly ITcxActivityService _activityService;

        public ActivityPageViewModel(INavigationService navigationService, ITcxActivityService activityService)
            : base(navigationService)
        {
            _activityService = activityService;

            Loader = new ViewModelLoader<SessionMapInfo>(emptyStateMessage: AppResources.EmptyActivityMessage);
        }

        public ViewModelLoader<SessionMapInfo> Loader { get; }

        public ActivityHeaderViewModel Header { get; private set; }

        public override void OnNavigatedTo(NavigationParameters parameters)
        {
            string activityId = parameters.GetValue<string>("activityId");

            var date = DateTime.ParseExact(activityId, "yyyyMMdd_HHmm", CultureInfo.InvariantCulture);
            Title = date.ToLongDateString();

            Loader.Load(() => LoadAsync(activityId));
        }

        private async Task<SessionMapInfo> LoadAsync(string activityId)
        {
            var activity = await _activityService.GetActivityAsync(activityId);

            if (activity.Lap[0].Track.Count < 2)
            {
                return null;
            }

            var activityPoints = activity.ToActivityPoints();

            Header = new ActivityHeaderViewModel(activity.ToActivityHeader(), new List<IDispersionSpan>());
            RaisePropertyChanged(nameof(Header));

            double maxSpeed = activity.Lap[0].MaximumSpeed * 3.6f;
            Color? SelectColorBySpeed(ISessionDisplayablePoint point)
            {
                if (point.Speed == null)
                {
                    return null;
                }

                return RunningEffortComputer.BySpeed.GetColor(point.Speed, maxSpeed);
            }

            Color? SelectColorByHeartRate(ISessionDisplayablePoint point)
            {
                if (point.HeartRate == null)
                {
                    return null;
                }

                return RunningEffortComputer.ByHeartBeat.GetColor(point.HeartRate);
            }

            if (Header.AverageHeartRate.HasValue)
            {
                return SessionMapInfo.Create(
                    activityPoints,
                    SelectColorByHeartRate,
                    1000,
                    1000);
            }

            return SessionMapInfo.Create(
                activityPoints,
                SelectColorBySpeed,
                1000,
                1000);
        }
    }
}