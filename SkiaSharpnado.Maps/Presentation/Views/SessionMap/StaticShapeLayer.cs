using System;

using SkiaSharp;

namespace SkiaSharpnado.Maps.Presentation.Views.SessionMap
{
    public class StaticShapeLayer<TShape> : AShape
        where TShape : IShape
    {
        private int _currentIndex = 0;

        public StaticShapeLayer(int shapeCount)
        {
            Layer = new IShape[shapeCount];
        }

        public bool HasShape => Layer[_currentIndex] != null;

        protected IShape[] Layer { get; }

        protected TimeSpan MaxTime { get; private set; }

        public void IncrementIndex()
        {
            _currentIndex++;
        }

        public override void UpdateOpacity(double opacity) => throw new NotSupportedException();

        public void Add(TShape shape)
        {
            Layer[_currentIndex] = shape;
        }

        public TShape GetCurrentShape()
        {
            return (TShape)Layer[_currentIndex];
        }

        public void UpdateMaxTime(TimeSpan time)
        {
            MaxTime = time;
        }

        public void ResetIndex()
        {
            _currentIndex = 0;
        }

        public override void Draw(SKCanvas canvas, SKPaint paint)
        {
            for (int index = 0; index < Layer.Length; index++)
            {
                IShape shape = Layer[index];
                if (shape == null || shape.Time > MaxTime)
                {
                    return;
                }

                shape.Draw(canvas, paint);
            }
        }

        protected override SKRect ComputeBoundBox()
        {
            throw new NotImplementedException();
        }
    }
}