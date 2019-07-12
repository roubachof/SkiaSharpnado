using System;
using System.Diagnostics;
using System.Linq;

using SkiaSharp;
using SkiaSharp.Views.Forms;

using SkiaSharpnado.Maps.Domain;
using SkiaSharpnado.Maps.Presentation.ViewModels.SessionMap;
using SkiaSharpnado.SkiaSharp;

using Xamarin.Forms;
using Xamarin.Forms.GoogleMaps;

namespace SkiaSharpnado.Maps.Presentation.Views.SessionMap
{
    public partial class SessionMap
    {
        public static readonly BindableProperty SessionMapInfoProperty = BindableProperty.Create(
            nameof(SessionMapInfo),
            typeof(SessionMapInfo),
            typeof(SessionMap),
            propertyChanged: SessionMapInfoChanged);

        public static readonly BindableProperty MaxTimeProperty = BindableProperty.Create(
            nameof(MaxTime),
            typeof(TimeSpan),
            typeof(SessionMap),
            defaultValue: TimeSpan.MaxValue,
            propertyChanged: MaxTimeChanged);

        private MarkerShapeLayer _markerLayer;

        private TextShapeLayer _textDistanceLayer;

        private PositionConverter _positionConverter;

        private bool _isCameraInitializing;
        private bool _isCameraInitialized;
        private bool _isMapLayoutOccured;
        private bool _isOverlayDrawn;

        private CameraPosition _movingCameraPosition;

        private LatLong _centerPosition;
        private LatLong _topLeftPosition;
        private LatLong _bottomRightPosition;

        private SKPoint _previousCenter;
        private double _previousTopLeftBottomRightSquareDistance;

        private SKPicture _overlayPicture;
        private SKMatrix _currentMatrix = SKMatrix.MakeIdentity();

        private SKPaint _markerPaint;
        private SKPaint _lastMarkerPaint;
        private SKPaint _gradientPathPaint;
        private SKPaint _distanceTextPaint;

        public SessionMap()
        {
            InitializeComponent();

            LayoutChanged += OnLayoutChanged;

            GoogleMap.UiSettings.ZoomControlsEnabled = false;
            GoogleMap.UiSettings.ZoomGesturesEnabled = true;
            GoogleMap.UiSettings.RotateGesturesEnabled = false;
            GoogleMap.UiSettings.TiltGesturesEnabled = false;
            GoogleMap.UiSettings.ScrollGesturesEnabled = true;

            GoogleMap.CameraMoving += CameraMoving;
        }

        public void OnDestroy()
        {
            LayoutChanged -= OnLayoutChanged;
            GoogleMap.CameraMoving -= CameraMoving;
        }

        public SessionMapInfo SessionMapInfo
        {
            get => (SessionMapInfo)GetValue(SessionMapInfoProperty);
            set => SetValue(SessionMapInfoProperty, value);
        }

        public TimeSpan MaxTime
        {
            get => (TimeSpan)GetValue(MaxTimeProperty);
            set => SetValue(MaxTimeProperty, value);
        }

        private static void SessionMapInfoChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            if (newvalue != null)
            {
                ((SessionMap)bindable).Initialize();
            }
        }

        private static void MaxTimeChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            ((SessionMap)bindable).MapOverlay.InvalidateSurface();
        }

        private void OnLayoutChanged(object sender, EventArgs e)
        {
            if (!_isCameraInitialized && GoogleMap.Bounds.Size != Size.Zero)
            {
                Debug.WriteLine($"OnLayoutChanged");
                _isMapLayoutOccured = true;

                InitializeMap();
            }
        }

        private void Initialize()
        {
            Debug.WriteLine($"Initialize");

            _positionConverter = new PositionConverter();

            int markerCount = SessionMapInfo.SessionPoints.Count(p => p.HasMarker);
            int textDistanceCount = SessionMapInfo.SessionPoints.Count(p => !string.IsNullOrEmpty(p.Label));

            MaxTime = TimeSpan.FromSeconds(SessionMapInfo.TotalDurationInSeconds);

            _markerLayer = null;
            _markerLayer = new MarkerShapeLayer(markerCount);

            _textDistanceLayer = null;
            _textDistanceLayer = new TextShapeLayer(textDistanceCount);

            var region = SessionMapInfo.Region;
            _centerPosition = new LatLong(region.Center.Latitude, region.Center.Longitude);
            _topLeftPosition = new LatLong(region.NorthWest.Latitude, region.NorthWest.Longitude);
            _bottomRightPosition = new LatLong(region.SouthEast.Latitude, region.SouthEast.Longitude);

            _previousCenter = SKPoint.Empty;
            _previousTopLeftBottomRightSquareDistance = 1;

            InitializeMap();
        }

        private void InitializeMap()
        {
            if (SessionMapInfo != null && _isMapLayoutOccured && !_isCameraInitializing)
            {
                Debug.WriteLine($"InitializeMap");

                if (GoogleMap.Bounds.Size != Size.Zero && Device.RuntimePlatform == Device.iOS)
                {
                    GoogleMap.MoveCamera(CameraUpdateFactory.NewBounds(SessionMapInfo.Region, 20));
                    _isCameraInitialized = true;
                    _isCameraInitializing = false;
                }
                else
                {
                    _isCameraInitializing = true;
                }

                if (Device.RuntimePlatform == Device.UWP)
                {
                    GoogleMap.CameraChanged += CameraChanged;
                }
                else
                {
                    GoogleMap.CameraIdled += CameraIdled;
                }
            }
        }

        private void CameraChanged(object sender, CameraChangedEventArgs e)
        {
            Debug.WriteLine($"CameraChanged");
            OnMapDisplayed();
        }

        private void CameraIdled(object sender, CameraIdledEventArgs e)
        {
            Debug.WriteLine($"CameraIdled");
            OnMapDisplayed();
        }

        private void OnMapDisplayed()
        {
            if (!_isCameraInitialized)
            {
                Debug.WriteLine($"RETURNING: !_isCameraInitialized");
                GoogleMap.MoveCamera(CameraUpdateFactory.NewBounds(SessionMapInfo.Region, 20));
                _isCameraInitialized = true;
                _isCameraInitializing = false;
                return;
            }

            _movingCameraPosition = null;
            MapOverlay.InvalidateSurface();
            
            Debug.WriteLine($"END OF => OnMapDisplayed: INVALIDATING");
        }

        private void CameraMoving(object sender, CameraMovingEventArgs e)
        {
            Debug.WriteLine($"CameraMoving: INVALIDATING");
            _movingCameraPosition = e.Position;
            MapOverlay.InvalidateSurface();
        }

        private void ReleaseMapResources()
        {
            _markerPaint?.Dispose();
            _markerPaint = null;

            _lastMarkerPaint?.Dispose();
            _lastMarkerPaint = null;

            _gradientPathPaint?.Dispose();
            _gradientPathPaint = null;

            _distanceTextPaint?.Dispose();
            _distanceTextPaint = null;
        }

        private void InitializeMapResourcesIfNeeded()
        {
            if (_gradientPathPaint != null)
            {
                return;
            }

            _gradientPathPaint = new SKPaint
            {
                Style = SKPaintStyle.StrokeAndFill,
                StrokeWidth = SkiaHelper.ToPixel(2),
            };

            _markerPaint = new SKPaint
            {
                IsAntialias = true,
                Color = SKColors.White,
            };

            _lastMarkerPaint = new SKPaint
            {
                Color = SKColors.White,
                StrokeWidth = SkiaHelper.ToPixel(2),
                Style = SKPaintStyle.Stroke,
            };

            _distanceTextPaint = new SKPaint
            {
                Color = SKColors.White,
                TextSize = SkiaHelper.ToPixel(12),
                IsAntialias = true,
            };
        }

        private void ApplyTransformation(SKCanvas canvas)
        {
            var centerPoint = _positionConverter[_centerPosition].ToSKPoint();
            var topLeftPoint = _positionConverter[_topLeftPosition].ToSKPoint();
            var bottomRightPoint = _positionConverter[_bottomRightPosition].ToSKPoint();

            var translation = centerPoint - _previousCenter;

            double squaredDistance = SKPoint.DistanceSquared(topLeftPoint, bottomRightPoint);
            double distanceRatio = squaredDistance / _previousTopLeftBottomRightSquareDistance;

            if (distanceRatio == 1 && translation == new SKPoint(0, 0))
            {
                return;
            }

            canvas.Clear();
            _previousCenter = centerPoint;
            _previousTopLeftBottomRightSquareDistance = squaredDistance;

            var transformMatrix = SKMatrix.MakeIdentity();
            transformMatrix.SetScaleTranslate((float)distanceRatio, (float)distanceRatio, translation.X, translation.Y);

            SKMatrix.Concat(ref _currentMatrix, _currentMatrix, transformMatrix);

            canvas.SetMatrix(_currentMatrix);
            canvas.DrawPicture(_overlayPicture);
        }

        private void MapOnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            // var info = e.RenderTarget;
            SKImageInfo info = e.Info;
            SKSurface surface = e.Surface;
            SKCanvas surfaceCanvas = surface.Canvas;

            var stopWatch = Stopwatch.StartNew();
            
            Debug.WriteLine($"MapOnPaintSurface");

            if (SessionMapInfo == null || !_isCameraInitialized)
            {
                Debug.WriteLine($"RETURNING: SessionMapInfo == null || !_isCameraInitialized");
                return;
            }

            if (_movingCameraPosition != null)
            {
                _positionConverter.UpdateCamera(
                    _movingCameraPosition.Target.ToLatLong(),
                    _movingCameraPosition.Zoom,
                    new Size(info.Width, info.Height),
                    SkiaHelper.PixelPerUnit);
            }
            else
            {
                _positionConverter.UpdateCamera(GoogleMap, new Size(info.Width, info.Height), SkiaHelper.PixelPerUnit);
            }

            var centerPoint = _positionConverter[_centerPosition].ToSKPoint();
            if (!_isOverlayDrawn && !info.Rect.Contains((int)centerPoint.X, (int)centerPoint.Y))
            {
                // Google maps hasn't finished initializing yet
                Debug.WriteLine($"RETURNING: Google maps hasn't finished initializing yet");
                _movingCameraPosition = null;
                return;
            }

            var topLeftPoint = _positionConverter[_topLeftPosition].ToSKPoint();
            var bottomRightPoint = _positionConverter[_bottomRightPosition].ToSKPoint();
            double squaredDistance = SKPoint.DistanceSquared(topLeftPoint, bottomRightPoint);

            if (_previousCenter == centerPoint && _previousTopLeftBottomRightSquareDistance == squaredDistance)
            {
                // Display view didn't changed
                Debug.WriteLine($"RETURNING: Display view didn't changed");
                _movingCameraPosition = null;
                return;
            }

            _previousCenter = centerPoint;
            _previousTopLeftBottomRightSquareDistance = squaredDistance;

            var pictureRecorder = new SKPictureRecorder();
            var canvas = pictureRecorder.BeginRecording(e.Info.Rect);

            InitializeMapResourcesIfNeeded();

            var sessionPoints = SessionMapInfo.SessionPoints;

            _markerLayer.ResetIndex();
            _textDistanceLayer.ResetIndex();

            SKPoint previousPoint = SKPoint.Empty;
            SKColor previousColor = SKColor.Empty;

            for (int index = 0; index < sessionPoints.Count; index++)
            {
                ISessionDisplayablePoint sessionPoint = sessionPoints[index];

                if (sessionPoint.Time > MaxTime)
                {
                    break;
                }

                SKPoint pathPoint = sessionPoint.Position != LatLong.Empty
                    ? _positionConverter[sessionPoint.Position].ToSKPoint()
                    : SKPoint.Empty;

                SKColor pointColor = sessionPoint.MapPointColor.ToSKColor();

                if (previousPoint != SKPoint.Empty && pathPoint != SKPoint.Empty)
                {
                    using (var shader = SKShader.CreateLinearGradient(
                        previousPoint,
                        pathPoint,
                        new[] { previousColor, pointColor },
                        null,
                        SKShaderTileMode.Clamp))
                    {
                        _gradientPathPaint.Shader = shader;

                        canvas.DrawLine(previousPoint.X, previousPoint.Y, pathPoint.X, pathPoint.Y, _gradientPathPaint);

                        _gradientPathPaint.Shader = null;
                    }
                }

                if (sessionPoint.HasMarker && previousPoint != SKPoint.Empty)
                {
                    if (!_markerLayer.HasShape)
                    {
                        _markerLayer.Add(new MarkerShape(sessionPoint.Time));
                    }

                    _markerLayer
                        .GetCurrentShape()
                        .UpdatePosition(previousPoint, pathPoint);

                    _markerLayer.IncrementIndex();
                }

                if (!string.IsNullOrEmpty(sessionPoint.Label))
                {
                    if (!_textDistanceLayer.HasShape)
                    {
                        _textDistanceLayer.Add(new TextShape(sessionPoint.Label, sessionPoint.Time));
                    }

                    _textDistanceLayer
                        .GetCurrentShape()
                        .UpdatePosition(
                            new SKPoint(pathPoint.X, pathPoint.Y + SkiaHelper.ToPixel(14)));

                    _textDistanceLayer.IncrementIndex();
                }

                previousPoint = pathPoint;
                previousColor = pointColor;
            }

            _markerLayer.UpdateMaxTime(MaxTime);
            _markerLayer.Draw(canvas, _markerPaint);

            _textDistanceLayer.UpdateMaxTime(MaxTime);
            _textDistanceLayer.Draw(canvas, _distanceTextPaint);

            DrawLastMarker(canvas, previousPoint);

            _isOverlayDrawn = true;

            ReleaseMapResources();

            _overlayPicture = pictureRecorder.EndRecording();

            surfaceCanvas.Clear();
            surfaceCanvas.DrawPicture(_overlayPicture);

            _overlayPicture.Dispose();
            pictureRecorder.Dispose();

            _movingCameraPosition = null;

            stopWatch.Stop();
            Debug.WriteLine($"END OF => MapOnPaintSurface ({stopWatch.Elapsed})");
        }

        private void DrawLastMarker(SKCanvas canvas, SKPoint lastPoint)
        {
            canvas.DrawCircle(lastPoint.X, lastPoint.Y, SkiaHelper.ToPixel(3), _lastMarkerPaint);
        }
    }
}