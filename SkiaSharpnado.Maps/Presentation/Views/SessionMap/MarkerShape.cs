using System;

using SkiaSharp;

using SkiaSharpnado.SkiaSharp;

namespace SkiaSharpnado.Maps.Presentation.Views.SessionMap
{
    public class MarkerShape : AShape
    {
        private int _arrowLength;
        private SKPoint _p1;
        private SKPoint _p2;

        private double _opacity = 1;

        public MarkerShape(TimeSpan time, int arrowLength)
        {
            Time = time;
            _arrowLength = arrowLength;
        }

        public MarkerShape UpdatePosition(SKPoint p1, SKPoint p2)
        {
            _p1 = p1;
            _p2 = p2;
            return this;
        }

        public void UpdateArrowLength(int arrowLength)
        {
            _arrowLength = arrowLength;
        }

        public override void UpdateOpacity(double opacity)
        {
            _opacity = opacity;
        }

        public override void Draw(SKCanvas canvas, SKPaint paint)
        {
            float vx = _p2.X - _p1.X;
            float vy = _p2.Y - _p1.Y;
            float dist = (float)Math.Sqrt(vx * vx + vy * vy);
            vx /= dist;
            vy /= dist;

            DrawArrowhead(canvas, paint, _p2, vx, vy, SkiaHelper.ToPixel(_arrowLength));
        }

        protected override SKRect ComputeBoundBox()
        {
            throw new NotImplementedException();
        }

        private void DrawArrowhead(SKCanvas canvas, SKPaint paint, SKPoint p, float nx, float ny, float length)
        {
            float ax = length * (-ny - nx);
            float ay = length * (nx - ny);
            SKPoint[] points =
            {
                new SKPoint(p.X + ax, p.Y + ay),
                p,
                new SKPoint(p.X - ay, p.Y + ax),
            };

            using (SKPath path = new SKPath())
            {
                path.MoveTo(points[0]);
                path.LineTo(points[1]);
                path.LineTo(points[2]);

                path.FillType = SKPathFillType.Winding;

                paint.Color = paint.Color.WithAlpha((byte)(_opacity * 255));
                canvas.DrawPath(path, paint);
            }
        }
    }
}