using System.Collections.Generic;

using Sample.Localization;

using SkiaSharpnado.Maps.Domain;

namespace Sample.Domain
{
    public static class HumanEffortComputer
    {
        public static EffortComputer ByHeartBeat { get; }

        public static EffortComputer BySpeed { get; }

        static HumanEffortComputer()
        {
            ByHeartBeat = new EffortComputer(
                new List<EffortSpan>
                    {
                        new EffortSpan(0f, ResourcesHelper.GetResourceColor("ColorEffortUnknown"), AppResources.PaceUnknown),
                        new EffortSpan(0.3f, ResourcesHelper.GetResourceColor("ColorEffortLightest"), AppResources.PaceVeryLight),
                        new EffortSpan(0.6f, ResourcesHelper.GetResourceColor("ColorEffortLight"), AppResources.PaceLight),
                        new EffortSpan(0.7f, ResourcesHelper.GetResourceColor("ColorEffortAerobic"), AppResources.PaceAerobic),
                        new EffortSpan(0.8f, ResourcesHelper.GetResourceColor("ColorEffortAnaerobic"), AppResources.PaceAnaerobic),
                        new EffortSpan(0.9f, ResourcesHelper.GetResourceColor("ColorEffortMax"), AppResources.PaceMax),
                    },
                180);

            BySpeed = new EffortComputer(
                new List<EffortSpan>
                    {
                        new EffortSpan(0f, ResourcesHelper.GetResourceColor("ColorEffortSpeedMin"), AppResources.PaceVeryLight),
                        new EffortSpan(0.7f, ResourcesHelper.GetResourceColor("ColorEffortAnaerobic"), AppResources.PaceAnaerobic),
                        new EffortSpan(0.95f, ResourcesHelper.GetResourceColor("ColorEffortMax"), AppResources.PaceMax),
                    },
                40);
        }
    }
}
