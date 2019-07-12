using SkiaSharp;

using Xamarin.Forms;

namespace SkiaSharpnado.SkiaSharp
{
    public enum DarkeningAmount
    {
        Unchanged = 0,
        Light = 10,
        Mild = 20,
        Strong = 30,
    }

    public static class SkiaExtensions
    {
        public static SKPoint ToPixelSKPoint(this Point formsPoint)
            => new SKPoint(SkiaHelper.ToPixel(formsPoint.X), SkiaHelper.ToPixel(formsPoint.Y));

        public static SKColor Darken(this SKColor color, DarkeningAmount amount = DarkeningAmount.Light)
        {
            color.ToHsl(out float h, out float s, out float l);
            return SKColor.FromHsl(h, s, l - (int)amount < 0 ? 0 : l - (int)amount);
        }

        public static SKColor Lighten(this SKColor color, DarkeningAmount amount = DarkeningAmount.Light)
        {
            color.ToHsl(out float h, out float s, out float l);
            return SKColor.FromHsl(h, s, l + (int)amount > 100 ? 100 : l + (int)amount);
        }
    }
}
