using System;
using System.Collections.Generic;

using Sample.Localization;

using SkiaSharpnado.Maps.Domain;

using Xamarin.Forms;

namespace Sample.Domain
{
    public static class RunningEffortComputer
    {
        public static EffortComputer ByHeartBeat { get; }

        public static EffortComputer BySpeed { get; }

        static RunningEffortComputer()
        {
            ByHeartBeat = new EffortComputer(
                new List<EffortSpan>
                    {
                        new EffortSpan(0f, GetResourceColor("ColorEffortUnknown"), AppResources.PaceUnknown),
                        new EffortSpan(0.3f, GetResourceColor("ColorEffortLightest"), AppResources.PaceVeryLight),
                        new EffortSpan(0.6f, GetResourceColor("ColorEffortLight"), AppResources.PaceLight),
                        new EffortSpan(0.7f, GetResourceColor("ColorEffortAerobic"), AppResources.PaceAerobic),
                        new EffortSpan(0.8f, GetResourceColor("ColorEffortAnaerobic"), AppResources.PaceAnaerobic),
                        new EffortSpan(0.9f, GetResourceColor("ColorEffortMax"), AppResources.PaceMax),
                    },
                180);

            BySpeed = new EffortComputer(
                new List<EffortSpan>
                    {
                        new EffortSpan(0f, GetResourceColor("ColorEffortSpeedMin"), AppResources.PaceVeryLight),
                        new EffortSpan(0.7f, GetResourceColor("ColorEffortAnaerobic"), AppResources.PaceAnaerobic),
                        new EffortSpan(0.95f, GetResourceColor("ColorEffortMax"), AppResources.PaceMax),
                    },
                40);
        }

        private static Color GetResourceColor(string key)
        {
            if (Application.Current.Resources.TryGetValue(key, out var value))
            {
                return (Color)value;
            }

            throw new InvalidOperationException($"key {key} not found in the resource dictionary");
        }
    }
}
