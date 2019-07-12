using Xamarin.Forms;

namespace SkiaSharpnado.ViewModels
{
    public interface IDispersionSpan
    {
        Color Color { get; }

        double Value { get; }

        void IncrementValue(double value);
    }

    public struct DispersionSpan : IDispersionSpan
    {
        public DispersionSpan(Color color, double value)
        {
            Color = color;
            Value = value;
        }

        public Color Color { get; }

        public double Value { get; private set; }

        public void IncrementValue(double value)
        {
            Value += value;
        }
    }
}
