using System;

using Xamarin.Forms;

namespace SkiaSharpnado.Maps.Domain
{
    public struct EffortSpan
    {
        public EffortSpan(double threshold, Color color, string label)
        {
            if (threshold < 0 || threshold > 1)
            {
                throw new ArgumentException("threshold >= 0 && threshold <= 1", nameof(threshold));
            }

            Threshold = threshold;
            Color = color;
            Label = label;
        }

        public double Threshold { get; }

        public Color Color { get; }

        public string Label { get; }
    }
}