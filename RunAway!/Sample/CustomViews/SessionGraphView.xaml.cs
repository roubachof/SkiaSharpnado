using System;
using System.Collections.Generic;
using Sample.ViewModels;
using Sample.Views;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using SkiaSharpnado.Maps.Presentation.ViewModels.SessionMap;
using SkiaSharpnado.SkiaSharp;
using TouchTracking;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Sample.CustomViews
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SessionGraphView : ContentView
    {
        public static readonly BindableProperty CurrentCursorTimeProperty = BindableProperty.Create(
            nameof(CurrentCursorTime),
            typeof(TimeSpan),
            typeof(ActivityPage),
            defaultValue: TimeSpan.Zero);

        public static readonly BindableProperty SessionGraphInfoProperty = BindableProperty.Create(
            nameof(SessionGraphInfo),
            typeof(SessionGraphInfo),
            typeof(SessionGraphView),
            propertyChanged: SessionGraphInfoChanged);

        private static readonly SKColor SpeedColor = ResourcesHelper.GetResourceColor("ColorGraphSpeed").ToSKColor();
        private static readonly SKColor BpmColor = ResourcesHelper.GetResourceColor("ColorGraphHeartRate").ToSKColor();
        private static readonly SKColor AltitudeColor = ResourcesHelper.GetResourceColor("ColorGraphAltitude").ToSKColor();
        private static readonly SKColor AltitudeSurface = ResourcesHelper.GetResourceColor("ColorGraphAltitudeSurface").ToSKColor();

        private SKPicture _curvesPicture;

        private SKPaint _overlayPaint;
        private SKPaint _cursorPaint;
        private SKPaint _curvePaint;
        private SKPaint _surfacePaint;
        private SKPaint _timeRectanglePaint;
        private SKPaint _timeTextPaint;

        public SessionGraphView()
        {
            InitializeComponent();
        }

        public ActivityPageViewModel ViewModel => BindingContext as ActivityPageViewModel;

        public TimeSpan CurrentCursorTime
        {
            get => (TimeSpan)GetValue(CurrentCursorTimeProperty);
            set => SetValue(CurrentCursorTimeProperty, value);
        }

        public SessionGraphInfo SessionGraphInfo
        {
            get => (SessionGraphInfo)GetValue(SessionGraphInfoProperty);
            set => SetValue(SessionGraphInfoProperty, value);
        }

        private static void SessionGraphInfoChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            var sessionGraphInfo = newvalue as SessionGraphInfo;
            if (newvalue != null)
            {
                ((SessionGraphView)bindable).Initialize(sessionGraphInfo);
            }
        }

        private void Initialize(SessionGraphInfo sessionGraphInfo)
        {
            SessionGraphInfo = sessionGraphInfo;
            CurrentCursorTime = TimeSpan.FromSeconds(sessionGraphInfo.TotalDurationInSeconds);
            Graph.InvalidateSurface();
        }

        private int TimeToPixels(TimeSpan time)
        {
            return (int)(Graph.CanvasSize.Width * time.TotalSeconds / SessionGraphInfo.TotalDurationInSeconds);
        }

        private void InitializeGraphResourcesIfNeeded()
        {
            if (_overlayPaint != null)
            {
                return;
            }

            _overlayPaint = new SKPaint
            {
                Color = ResourcesHelper.GetResourceColor("ColorGraphOverlay").ToSKColor(),
                Style = SKPaintStyle.Fill,
            };

            _cursorPaint = new SKPaint
            {
                Color = SKColors.White,
                Style = SKPaintStyle.Fill,
                StrokeWidth = SkiaHelper.ToPixel(1.5),
            };

            _curvePaint = new SKPaint
            {
                StrokeWidth = SkiaHelper.ToPixel(2),
                Style = SKPaintStyle.Stroke,
                StrokeJoin = SKStrokeJoin.Round,
                IsAntialias = true,
            };

            _surfacePaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                IsAntialias = true,
            };

            _timeRectanglePaint = new SKPaint
            {
                Color = SKColors.White,
                Style = SKPaintStyle.Fill,
            };

            _timeTextPaint = new SKPaint
            {
                Color = SKColors.Black,
                TextSize = SkiaHelper.ToPixel(13),
                IsAntialias = true,
            };
        }

        private void ReleaseGraphResources()
        {
            _overlayPaint?.Dispose();
            _overlayPaint = null;

            _cursorPaint?.Dispose();
            _cursorPaint = null;

            _curvePaint?.Dispose();
            _curvePaint = null;

            _surfacePaint?.Dispose();
            _surfacePaint = null;

            _timeRectanglePaint?.Dispose();
            _timeRectanglePaint = null;

            _timeTextPaint?.Dispose();
            _timeTextPaint = null;
        }

        private void DrawCurve(
            SKCanvas canvas,
            IReadOnlyList<ISessionDisplayablePoint> sessionPoints,
            ValueBounds valueBounds,
            SKColor color,
            Func<ISessionDisplayablePoint, double?> valueGetter)
        {
            if (sessionPoints.Count == 0)
            {
                return;
            }

            double secondsPerPixel = SessionGraphInfo.TotalDurationInSeconds / Width;

            _curvePaint.Color = color;

            using (var curvePath = new SKPath())
            {
                bool isMoveNext = true;
                double currentSeconds = 0;
                for (int index = 0; index < sessionPoints.Count; index++)
                {
                    var sessionPoint = sessionPoints[index];

                    var value = valueGetter(sessionPoint);

                    if (sessionPoint.Time.TotalSeconds < currentSeconds)
                    {
                        continue;
                    }

                    if (!value.HasValue || value == 0f)
                    {
                        isMoveNext = true;
                        continue;
                    }

                    if (isMoveNext)
                    {
                        curvePath.MoveTo(new SKPoint(GetX(sessionPoint, SessionGraphInfo.TotalDurationInSeconds), GetY(valueBounds, value)));

                        isMoveNext = false;
                        continue;
                    }

                    curvePath.LineTo(new SKPoint(GetX(sessionPoint, SessionGraphInfo.TotalDurationInSeconds), GetY(valueBounds, value)));
                    currentSeconds += secondsPerPixel;
                }

                if (curvePath.PointCount < 2)
                {
                    return;
                }

                using (var shader = SKShader.CreateLinearGradient(
                    curvePath.Points[0],
                    curvePath.LastPoint,
                    new[] { color.Darken(), color },
                    null,
                    SKShaderTileMode.Clamp))
                {
                    _curvePaint.Shader = shader;
                    canvas.DrawPath(curvePath, _curvePaint);
                }
            }
        }

        private void DrawSurface(
            SKCanvas canvas,
            IReadOnlyList<ISessionDisplayablePoint> sessionPoints,
            ValueBounds valueBounds,
            SKColor color,
            Func<ISessionDisplayablePoint, double?> valueGetter)
        {
            if (sessionPoints.Count == 0)
            {
                return;
            }

            double secondsPerPixel = SessionGraphInfo.TotalDurationInSeconds / Width;

            _surfacePaint.Color = color;

            SKPoint bottomLeft = new SKPoint(0, SkiaHelper.ToPixel(Height));
            SKPoint bottomRight = new SKPoint(SkiaHelper.ToPixel(Width), SkiaHelper.ToPixel(Height));

            using (var curvePath = new SKPath())
            {
                curvePath.MoveTo(bottomLeft);

                double currentSeconds = 0;
                for (int index = 0; index < sessionPoints.Count; index++)
                {
                    var sessionPoint = sessionPoints[index];

                    var value = valueGetter(sessionPoint);

                    if (sessionPoint.Time.TotalSeconds < currentSeconds)
                    {
                        continue;
                    }

                    curvePath.LineTo(new SKPoint(GetX(sessionPoint, SessionGraphInfo.TotalDurationInSeconds), GetY(valueBounds, value)));
                    currentSeconds += secondsPerPixel;
                }

                if (curvePath.PointCount < 2)
                {
                    return;
                }

                curvePath.LineTo(bottomRight);
                curvePath.Close();

                using (var shader = SKShader.CreateLinearGradient(
                    curvePath.Points[0],
                    curvePath.LastPoint,
                    new[] { color, color.Darken() },
                    null,
                    SKShaderTileMode.Clamp))
                {
                    _surfacePaint.Shader = shader;
                    canvas.DrawPath(curvePath, _surfacePaint);
                }
            }
        }


        private void GraphOnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            // var info = e.RenderTarget;
            SKSurface surface = e.Surface;
            SKCanvas surfaceCanvas = surface.Canvas;

            var sessionGraphInfo = SessionGraphInfo;

            if (sessionGraphInfo == null)
            {
                return;
            }

            InitializeGraphResourcesIfNeeded();

            if (_curvesPicture == null)
            {
                var pictureRecorder = new SKPictureRecorder();
                var canvas = pictureRecorder.BeginRecording(e.Info.Rect);

                //DrawCurve(
                //    canvas,
                //    sessionGraphInfo.SessionPoints,
                //    sessionGraphInfo.Altitude,
                //    AltitudeColor,
                //    sessionPoint => sessionPoint.Altitude);

                DrawSurface(
                    canvas,
                    sessionGraphInfo.SessionPoints,
                    sessionGraphInfo.Altitude,
                    AltitudeSurface,
                    sessionPoint => sessionPoint.Altitude);

                DrawCurve(
                    canvas,
                    sessionGraphInfo.SessionPoints,
                    sessionGraphInfo.Speed,
                    SpeedColor,
                    sessionPoint => sessionPoint.Speed);

                DrawCurve(
                    canvas,
                    sessionGraphInfo.SessionPoints,
                    sessionGraphInfo.HeartRate,
                    BpmColor,
                    sessionPoint => sessionPoint.HeartRate);

                _curvesPicture = pictureRecorder.EndRecording();
                pictureRecorder.Dispose();
            }

            surfaceCanvas.Clear();
            surfaceCanvas.DrawPicture(_curvesPicture);

            int cursorX = TimeToPixels(CurrentCursorTime);
            surfaceCanvas.DrawRect(new SKRect(0, 0, cursorX, Graph.CanvasSize.Height), _overlayPaint);
            surfaceCanvas.DrawLine(cursorX, 0, cursorX, Graph.CanvasSize.Height, _cursorPaint);

            DrawArrows(surfaceCanvas, _cursorPaint, cursorX);
            DrawTime(surfaceCanvas, cursorX);

            ReleaseGraphResources();
        }

        private void DrawArrows(SKCanvas canvas, SKPaint paint, float x)
        {
            float arrowHeight = SkiaHelper.ToPixel(8);
            float arrowWidth = SkiaHelper.ToPixel(8);
            float horizontalMargin = SkiaHelper.ToPixel(2);
            float verticalMargin = SkiaHelper.ToPixel(5);

            SKPoint[] leftArrowPoints =
            {
                new SKPoint(x - horizontalMargin, verticalMargin),
                new SKPoint(x - horizontalMargin - arrowWidth, verticalMargin + arrowHeight / 2),
                new SKPoint(x - horizontalMargin, verticalMargin + arrowHeight),
            };

            using (var path = new SKPath())
            {
                path.MoveTo(leftArrowPoints[0]);
                path.LineTo(leftArrowPoints[1]);
                path.LineTo(leftArrowPoints[2]);

                path.FillType = SKPathFillType.Winding;
                canvas.DrawPath(path, paint);
            }

            SKPoint[] rightArrowPoints =
            {
                new SKPoint(x + horizontalMargin, verticalMargin),
                new SKPoint(x + horizontalMargin + arrowWidth, verticalMargin + arrowHeight / 2),
                new SKPoint(x + horizontalMargin, verticalMargin + arrowHeight),
            };

            using (var path = new SKPath())
            {
                path.MoveTo(rightArrowPoints[0]);
                path.LineTo(rightArrowPoints[1]);
                path.LineTo(rightArrowPoints[2]);

                path.FillType = SKPathFillType.Winding;
                canvas.DrawPath(path, paint);
            }
        }

        private void DrawTime(SKCanvas canvas, float cursorX)
        {
            int cursorY = (int)(Graph.CanvasSize.Height / 2);

            string timeText = $"{CurrentCursorTime.Minutes:00}:{CurrentCursorTime.Seconds:00}";

            if (CurrentCursorTime >= TimeSpan.FromHours(1))
            {
                timeText = timeText.Insert(0, $"{CurrentCursorTime.Hours}:");
            }

            SKRect textBounds = SKRect.Empty;

            _timeTextPaint.MeasureText(timeText, ref textBounds);

            float rectangleHPadding = SkiaHelper.ToPixel(3);
            float rectangleVPadding = SkiaHelper.ToPixel(2);

            float leftText = cursorX - textBounds.Width / 2;
            float rightText = cursorX + textBounds.Width / 2;

            float topText = cursorY - textBounds.Height / 2;
            float bottomText = cursorY + textBounds.Height / 2;

            float leftRectangle = leftText - rectangleHPadding;
            float rightRectangle = rightText + rectangleHPadding;
            float topRectangle = topText - rectangleVPadding;
            float bottomRectangle = bottomText + rectangleVPadding;

            if (leftRectangle < 0)
            {
                rightRectangle += -leftRectangle;
                leftRectangle = 0;
                leftText = rectangleHPadding;
            }
            else if (rightRectangle > Graph.CanvasSize.Width)
            {
                float delta = rightRectangle - Graph.CanvasSize.Width;
                leftRectangle -= delta;
                rightRectangle = Graph.CanvasSize.Width;
                leftText = rightRectangle - (textBounds.Width + rectangleHPadding);
            }

            var timeRectangle = new SKRect(
                leftRectangle,
                topRectangle,
                rightRectangle,
                bottomRectangle);

            canvas.DrawRoundRect(timeRectangle, SkiaHelper.ToPixel(3), SkiaHelper.ToPixel(3), _timeRectanglePaint);
            canvas.DrawText(timeText, leftText, cursorY - textBounds.MidY, _timeTextPaint);
        }

        private float GetX(ISessionDisplayablePoint displayPoint, int totalDuration)
        {
            return Graph.CanvasSize.Width * (int)displayPoint.Time.TotalSeconds / totalDuration;
        }

        private float GetY(ValueBounds valueBounds, double? value)
        {
            const int verticalPadding = 10;

            if (!value.HasValue)
            {
                return (float)valueBounds.Min;
            }

            double minValue = valueBounds.Min;
            float bpmMaxDelta = (float)(valueBounds.Max - minValue);
            if (bpmMaxDelta < 1)
            {
                minValue = valueBounds.Max * 0.25;
                bpmMaxDelta = (float)(valueBounds.Max - minValue);
            }

            float drawableCanvasHeight = Graph.CanvasSize.Height - 2 * SkiaHelper.ToPixel(verticalPadding);

            float inversedY = drawableCanvasHeight * ((float)value.Value - (float)minValue) / bpmMaxDelta;

            return SkiaHelper.ToPixel(verticalPadding) + Math.Abs(drawableCanvasHeight - inversedY);
        }

        private void OnTouchEffectAction(object sender, TouchActionEventArgs args)
        {
            if (args.Type == TouchActionType.Moved && SessionGraphInfo != null)
            {
                float positionX = SkiaHelper.ToPixel(args.Location.X);

                CurrentCursorTime = TimeSpan.FromSeconds(
                    ComputationHelper.Clamp(
                        SessionGraphInfo.TotalDurationInSeconds * positionX / Graph.CanvasSize.Width,
                        0,
                        SessionGraphInfo.TotalDurationInSeconds));

                Graph.InvalidateSurface();
            }
        }
    }
}