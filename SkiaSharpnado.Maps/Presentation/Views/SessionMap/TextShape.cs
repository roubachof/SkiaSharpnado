using System;

using SkiaSharp;

namespace SkiaSharpnado.Maps.Presentation.Views.SessionMap
{
    public class TextShape : ASinglePointShape
    {
        private readonly string _textDistance;

        private double _opacity = 1;

        public TextShape(string textDistance, TimeSpan time)
        {
            _textDistance = textDistance;

            Time = time;
        }

        public void UpdatePosition(SKPoint point)
        {
            Point = point;
        }

        public override void UpdateOpacity(double opacity)
        {
            _opacity = opacity;
        }

        public override void Draw(SKCanvas canvas, SKPaint paint)
        {
            paint.Color = paint.Color.WithAlpha((byte)(_opacity * 255));
            canvas.DrawText(_textDistance, Point.X, Point.Y, paint);
        }

        protected override SKRect ComputeBoundBox()
        {
            throw new NotImplementedException();
        }
    }
}