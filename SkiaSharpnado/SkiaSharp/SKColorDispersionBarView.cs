using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using SkiaSharp;
using SkiaSharp.Views.Forms;

using SkiaSharpnado.ViewModels;

using Xamarin.Forms;

namespace SkiaSharpnado.SkiaSharp
{
    public class SKColorDispersionBarView : SKCanvasView
    {
        public static readonly BindableProperty DispersionProperty =
            BindableProperty.Create(
                nameof(Dispersion),
                typeof(List<IDispersionSpan>),
                typeof(SKColorDispersionBarView),
                defaultValue: null,
                propertyChanged: DispersionPropertyChanged);

        public List<IDispersionSpan> Dispersion
        {
            get => (List<IDispersionSpan>)GetValue(DispersionProperty);
            set => SetValue(DispersionProperty, value);
        }
        private static void DispersionPropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            var barView = (SKColorDispersionBarView)bindable;
            barView.InvalidateSurface();
        }

        protected override void OnPaintSurface(SKPaintSurfaceEventArgs e)
        {
            base.OnPaintSurface(e);

            SKSurface surface = e.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear();

            if (Dispersion == null || Dispersion.Count == 0)
            {
                return;
            }

            float width = CanvasSize.Width;
            float height = CanvasSize.Height;
            double totalCount = Dispersion.Sum(d => d.Value);
            float currentX = 0;

            width -= SkiaHelper.ToPixel(Dispersion.Count - 1);

            using (var paint = new SKPaint { Style = SKPaintStyle.Fill })
            {
                foreach (var dispersionSpan in Dispersion)
                {
                    double rectangleWidth = width * dispersionSpan.Value / totalCount;

                    SKColor effortStartColor = dispersionSpan.Color.ToSKColor();
                    SKColor effortTargetColor = effortStartColor.Darken();

                    var upperLeft = new SKPoint(currentX, 0);
                    var bottomRight = new SKPoint(currentX + (float)rectangleWidth, height);

                    using (var shader = SKShader.CreateLinearGradient(
                        upperLeft,
                        bottomRight,
                        new[] { effortStartColor, effortTargetColor },
                        null,
                        SKShaderTileMode.Clamp))
                    {
                        paint.Shader = shader;

                        canvas.DrawRect(new SKRect(upperLeft.X, upperLeft.Y, bottomRight.X, bottomRight.Y), paint);
                    }

                    currentX += (float)rectangleWidth + 1;
                }
            }
        }
    }
}
