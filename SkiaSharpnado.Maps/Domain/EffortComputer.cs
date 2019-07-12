using System;
using System.Collections.Generic;
using System.Linq;

using Xamarin.Forms;

namespace SkiaSharpnado.Maps.Domain
{
    public class EffortComputer
    {
        private readonly List<EffortSpan> _effortSpans;

        private double _defaultMaxEffortValue;

        public EffortComputer(List<EffortSpan> effortSpans, double defaultMaxEffortValue)
        {
            if (effortSpans.Count < 2)
            {
                throw new ArgumentException("effortSpans.Count >= 2", nameof(effortSpans));
            }

            _effortSpans = effortSpans;
            _defaultMaxEffortValue = defaultMaxEffortValue;

            LastSpan = _effortSpans.Last();
        }

        public EffortSpan LastSpan { get; }

        public EffortComputer OverrideDefaultMaxValue(double defaultMaxValue)
        {
            _defaultMaxEffortValue = defaultMaxValue;
            return this;
        }

        public EffortSpan GetSpan(double? effortValue)
        {
            double currentPercentage = (effortValue ?? 0) / _defaultMaxEffortValue;

            EffortSpan previousSpan = _effortSpans[0];
            foreach (var currentSpan in _effortSpans)
            {
                if (currentPercentage < currentSpan.Threshold)
                {
                    return previousSpan;
                }

                previousSpan = currentSpan;
            }

            return LastSpan;
        }

        public Color GetColor(double? effortValue, double? maxEffortValue = null)
        {
            double currentPercentage = (effortValue ?? 0) / (maxEffortValue ?? _defaultMaxEffortValue);

            if (currentPercentage >= LastSpan.Threshold)
            {
                return LastSpan.Color;
            }

            var sourceSpan = _effortSpans[0];
            var targetSpan = _effortSpans[1];

            EffortSpan previousSpan = _effortSpans[0];
            foreach (var currentSpan in _effortSpans)
            {
                sourceSpan = previousSpan;
                targetSpan = currentSpan;

                if (currentPercentage < currentSpan.Threshold)
                {
                    break;
                }

                previousSpan = currentSpan;
            }

            double percentToTarget =
                (currentPercentage - sourceSpan.Threshold) / (targetSpan.Threshold - sourceSpan.Threshold);

            var sourceColor = sourceSpan.Color;
            var targetColor = targetSpan.Color;

            // Define color
            return Color.FromRgba(
                sourceColor.R + (percentToTarget * (targetColor.R - sourceColor.R)),
                sourceColor.G + (percentToTarget * (targetColor.G - sourceColor.G)),
                sourceColor.B + (percentToTarget * (targetColor.B - sourceColor.B)),
                sourceColor.A + (percentToTarget * (targetColor.A - sourceColor.A)));
        }
    }
}