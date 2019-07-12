using System;

using SkiaSharp;

namespace SkiaSharpnado.Maps.Presentation.Views.SessionMap
{
    public class TextShapeLayer : StaticShapeLayer<TextShape>
    {
        public TextShapeLayer(int shapeCount)
            : base(shapeCount)
        {
        }

        public override void Draw(SKCanvas canvas, SKPaint paint)
        {
            if (Layer.Length == 0 || Layer[0].Time > MaxTime)
            {
                return;
            }

            int lastDrawableShapeIndex = Array.FindIndex(Layer, text => text.Time > MaxTime);
            lastDrawableShapeIndex = lastDrawableShapeIndex == -1 ? Layer.Length - 1 : lastDrawableShapeIndex - 1;

            lastDrawableShapeIndex = lastDrawableShapeIndex < 0 ? 0 : lastDrawableShapeIndex;

            DrawDistances(canvas, paint, lastDrawableShapeIndex, lastDrawableShapeIndex + 1);

            // DrawLastDistance(canvas, paint, lastDrawableShapeIndex);
        }

        private void DrawLastDistance(SKCanvas canvas, SKPaint paint, int lastDrawableShapeIndex)
        {
            if (Layer.Length == 0)
            {
                return;
            }

            IShape shape = Layer[lastDrawableShapeIndex];
            shape.Draw(canvas, paint);
        }

        private void DrawDistances(SKCanvas canvas, SKPaint paint, int lastDrawableShapeIndex, int lastMarkers)
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
