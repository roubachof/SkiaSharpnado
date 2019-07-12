using System;

using SkiaSharp;

namespace SkiaSharpnado.Maps.Presentation.Views.SessionMap
{
    public class MarkerShapeLayer : StaticShapeLayer<MarkerShape>
    {
        public MarkerShapeLayer(int shapeCount)
            : base(shapeCount)
        {
        }

        public override void Draw(SKCanvas canvas, SKPaint paint)
        {
            if (Layer.Length == 0 || Layer[0].Time > MaxTime)
            {
                return;
            }

            int lastDrawableShapeIndex = Array.FindIndex(Layer, marker => marker != null && marker.Time > MaxTime);
            lastDrawableShapeIndex = lastDrawableShapeIndex == -1 ? Layer.Length - 1 : lastDrawableShapeIndex - 1;

            DrawLastMarkers(canvas, paint, lastDrawableShapeIndex, lastDrawableShapeIndex + 1);
        }

        private void DrawLastMarkers(SKCanvas canvas, SKPaint paint, int lastDrawableShapeIndex, int lastMarkers)
        {
            int startIndex = lastDrawableShapeIndex - lastMarkers;
            startIndex = startIndex < 0 ? 0 : startIndex;

            float opacityStep = 0.9f / lastMarkers;
            float opacity = opacityStep;

            for (int index = startIndex; index < Layer.Length; index++)
            {
                IShape shape = Layer[index];
                if (shape == null || shape.Time > MaxTime)
                {
                    return;
                }

                shape.UpdateOpacity(opacity);
                opacity += opacityStep;

                shape.Draw(canvas, paint);
            }
        }
    }
}