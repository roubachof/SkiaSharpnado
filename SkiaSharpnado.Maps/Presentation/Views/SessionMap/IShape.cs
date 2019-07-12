using System;

using SkiaSharp;

namespace SkiaSharpnado.Maps.Presentation.Views.SessionMap
{
    public interface IShape
    {
        TimeSpan Time { get; }

        SKRect BoundingBox { get; }

        void UpdateOpacity(double opacity);

        void Draw(SKCanvas canvas, SKPaint paint);
    }
}