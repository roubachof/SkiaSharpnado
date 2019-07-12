using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using Prism.Navigation;

using Sample.Domain;
using Sample.Localization;

using Sharpnado.Presentation.Forms.ViewModels;

using SkiaSharpnado.Maps.Domain;
using SkiaSharpnado.ViewModels;

using TcxTools;

using Xamarin.Forms;

namespace Sample.ViewModels
{
    public class ActivityHeaderPageViewModel : ViewModelBase
    {
        private readonly ITcxActivityService _activityService;

        public ActivityHeaderPageViewModel(INavigationService navigationService, ITcxActivityService activityService)
            : base(navigationService)
        {
            _activityService = activityService;

            Loader = new ViewModelLoader<List<ActivityHeaderViewModel>>(emptyStateMessage: AppResources.EmptyActivityMessage);
            ActivityTappedCommand = new Command<ActivityHeaderViewModel>(
                item => NavigationService.NavigateAsync($"ActivityPage?activityId={item.Id}"));
        }

        public ICommand ActivityTappedCommand { get; }

        public ViewModelLoader<List<ActivityHeaderViewModel>> Loader { get; }

        public override void OnNavigatedTo(NavigationParameters parameters)
        {
            Title = AppResources.ActivityHeaderPageTitle;

            if (Loader.IsSuccessfullyCompleted)
            {
                return;
            }

            Loader.Load(LoadAsync);
        }

        private async Task<List<ActivityHeaderViewModel>> LoadAsync()
        {
            var activities = await _activityService.GetActivitiesAsync();
            var result = new List<ActivityHeaderViewModel>();
            foreach (var activity in activities)
            {
                var header = await CreateHeaderViewModelAsync(activity);
                result.Add(header);
            }

            return result;
        }

        private Task<ActivityHeaderViewModel> CreateHeaderViewModelAsync(Activity activity)
        {
            return Task.Run(
                () =>
                    {
                        var lap = activity.Lap[0];

                        double maxSpeed = lap.MaximumSpeed * 3.6;
                        EffortComputer effortComputer = lap.AverageHeartRateBpm != null
                            ? RunningEffortComputer.ByHeartBeat
                            : RunningEffortComputer.BySpeed.OverrideDefaultMaxValue(maxSpeed);

                        var dispersion = new SortedDictionary<double, IDispersionSpan>();
                        Trackpoint previousPoint = lap.Track[0];
                        DateTime startTime = lap.Track[0].Time;
                        for (int index = 0; index < lap.Track.Count; index++)
                        {
                            Trackpoint currentPoint = lap.Track[index];

                            TimeSpan elapsedTime = currentPoint.Time - startTime;

                            double? speed = null;
                            if (previousPoint != null && previousPoint.Position != null
                                && previousPoint.DistanceMeters > 0 && currentPoint.Position != null
                                && currentPoint.DistanceMeters > 0 && elapsedTime.TotalSeconds > 0)
                            {
                                double kilometersTraveled = GeoCalculation.HaversineDistance(
                                    new LatLong(
                                        previousPoint.Position.LatitudeDegrees,
                                        previousPoint.Position.LongitudeDegrees),
                                    new LatLong(
                                        currentPoint.Position.LatitudeDegrees,
                                        currentPoint.Position.LongitudeDegrees));
                                double hoursElapsed = (elapsedTime - (previousPoint.Time - startTime)).TotalHours;
                                speed = kilometersTraveled / hoursElapsed;
                            }

                            double? value = lap.AverageHeartRateBpm != null ? currentPoint.HeartRateBpm?.Value : speed;
                            if (value == null)
                            {
                                previousPoint = currentPoint;
                                continue;
                            }

                            var elapsedTimeSinceLastPoint = currentPoint.Time - previousPoint.Time;
                            EffortSpan effortSpan = effortComputer.GetSpan(value);
                            if (!dispersion.ContainsKey(effortSpan.Threshold))
                            {
                                dispersion.Add(effortSpan.Threshold, new DispersionSpan(effortSpan.Color, 0));
                            }

                            dispersion[effortSpan.Threshold]
                                .IncrementValue(elapsedTimeSinceLastPoint.TotalMilliseconds);

                            previousPoint = currentPoint;
                        }

                        return new ActivityHeaderViewModel(activity.ToActivityHeader(), dispersion.Values.ToList());
                    });
        }
    }
}