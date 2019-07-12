using System;

using SkiaSharp;

namespace SkiaSharpnado.SkiaSharp
{
    public static class SkiaHelper
    {
        public static float PixelPerUnit { get; private set; }

        public static bool IsInitialized => PixelPerUnit > 0f;

        public static void Initialize(float pixelsPerUnit)
        {
            PixelPerUnit = pixelsPerUnit;
        }

        public static Double ToDp(float pixels)
        {
            return pixels / PixelPerUnit;
        }

        public static float ToPixel(float dip)
        {
            return dip * PixelPerUnit;
        }

        public static float ToPixel(double dip)
        {
            return ToPixel((float)dip);
        }

        public static float ToPixel(int dip)
        {
            return ToPixel((float)dip);
        }

        public static bool AreColorsClosed(SKColor color1, SKColor color2)
        {
            var delta = Math.Abs(color1.Hue - color2.Hue);
            if (delta > 180)
            {
                delta = 360 - delta;
            }

            return delta <= 10;
        }
    }
}