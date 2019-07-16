# SkiaSharpnado: SkiaSharp components and case studies for Xamarin.Forms

If you want real details on this, please see my blog post on Sharpnado: https://www.sharpnado.com/run-away-app

For now, you will find two netstandard projects ready to be reused:

1. ```SkiaSharpnado```
2. ```SkiaSharpnado.Maps```

In the near future, I will release some SkiaSharp components through a SkiaSharpnado nuget package.

The ```SkiaSharpnado.Maps``` project contains all the bits to draw the gradient paths on Maps.
It may be also released as a nuget package: it will depend on if you want to :)

Those projects are used by the ```Sample``` project which is a ```Xamarin.Forms``` app targeting, iOS, Android AND UWP.
Yes, I didn't forget **UWP** this time, even if the result is not as good as the others platorm, but stay tuned for the disappointement :)

## Run Away! A training app displaying gradient lines on top of Google.Maps

So the activities you will see are extracted from real people running apps:

* [David "Big Heart" Ortinau](https://twitter.com/davidortinau)
* [Glenn "Motocycle" Versweyveld](https://twitter.com/Depechie)
* [Steven "Indurain" Thewisen](https://twitter.com/devnl)

Thanks again to those great dedicated souls!

You can even export you own activities as TCX files, and add them in the Resources folder, it will pick them automatically.

**Remark:** I used [Google Material Dark theme](https://material.io/design/color/dark-theme.html) to design the app.

### The ActivityHeaderPage

<table>
	<thead>
		<tr>
			<th>Android</th>
			<th>iOS</th>
      <th>UWP</th>
		</tr>
	</thead>
	<tbody>
		<tr>
			<td><img src="__Docs__/activity_header_page_android.png" width="300" /></td>
			<td><img src="__Docs__/activity_header_page_ios.png" width="300" /></td>
      <td><img src="__Docs__/activity_header_page_uwp.png" width="300" /></td>
		</tr>
  </tbody>
</table>

I used text semantic and followed the elevation color guidelines of the material design.

I used Font Awesome for the icons (thanks Xappy for the tutorial ;).

The only special thing about this screen is the little colored bar you can see below the activity title. These are not random colors. There are in fact the effort dispersion based on the heart rates of each activity.
The effort colors are defined in the ```Colors.xaml``` file:

![](__Docs__/effort_colors.png)

* We can see that Steven's activity is the most balanced, it's quite an adventure, 7 hours of cycling. Steven climbed the Joux-Plane Pass, which is a famous stage of the "Tour de France". There were some calm moments, flat segments, going down the pass, and some really challenging ones: obviously going up the pass (9% average, 11km).

* David entry is really heart intensive with more than 50% of the time close to max heart rate. He went running on a very hot day at the beginning of the afternoon on a steep road (thank you again for your sacrifice :).

* Finally, Glenn pushed hard on the pedals on a rather flat area to achieve nearly 30 km/h on average. The effort was constant, quite strong (most of it qualifies as anaerobic span), with little variations.

#### SKColorDispersionBarView

So this bar is made with... SKIASHARP obviously, and the code is pretty simple.

```csharp
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
```

The dispersion is computed in the ```ActivityHeaderPageViewModel```. What I call dispersion, is a list of DispersionSpan. In our app case, the DispersionSpan color is the effort color (given by the heart rate), and the value the total ms time spent in this effort interval.

### The ActivityPage

And now ladies and gentlemen, the long awaited moment, let's draw some gradient lines on Maps!

<table>
	<thead>
		<tr>
			<th>Android (Steven)</th>
			<th>iOS (David)</th>
      <th>UWP (Glenn)</th>
		</tr>
	</thead>
	<tbody>
		<tr>
			<td><img src="__Docs__/activity_page_android_4dp.png" width="300" /></td>
			<td><img src="__Docs__/activity_page_ios_4dp.png" width="300" /></td>
      <td><img src="__Docs__/activity_page_uwp_4dp.png" width="300" /></td>
		</tr>
  </tbody>
</table>

Functionnally speaking, the colors displayed on the map are computed the same way than the ```SKColorDispersionBarView```: by the ```HumanEffortComputer```. The only thing that changed is that we interpolate the color to give that nice gradient touch. The start and end point are marked by icons colorized by the athlete heartrate at this precise moment.

* Steven's map is more colorful as we see earlier. Calories weren't exported for some reason, I'm thinking int overflow. Lots of heartrate variations, we can see clearly the top of the Pass, the effort goes from max (red) to light (blue). I suspect a quick break for a Pastis.
* David's path is painted in max effort with an impressive average rate of 159 bpm (I think I would have died at minute 3).
* Glenn track is yellow-orangish with a constant effort on a disappointing UWP implementation.

### The SessionMap view (SkiaSharpnado.Maps.Views)

This is our main piece.
It's made of the GoogleMaps component from the legendary [amay077](https://github.com/amay077), and a simple ```SkiaSharp``` overlay.

```xml
<?xml version="1.0" encoding="UTF-8"?>
<ContentView xmlns="http://xamarin.com/schemas/2014/forms"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:forms="clr-namespace:SkiaSharp.Views.Forms;assembly=SkiaSharp.Views.Forms"
             xmlns:googleMaps="clr-namespace:Xamarin.Forms.GoogleMaps;assembly=Xamarin.Forms.GoogleMaps"
             x:Class="SkiaSharpnado.Maps.Presentation.Views.SessionMap.SessionMap">
    <ContentView.Content>
        <Grid RowSpacing="0">
            <googleMaps:Map Grid.Row="0"
                            x:Name="GoogleMap"
                            MapType="Satellite" />

            <forms:SKCanvasView Grid.Row="0"
                                x:Name="MapOverlay"
                                InputTransparent="True"
                                PaintSurface="MapOnPaintSurface" />
        </Grid>
    </ContentView.Content>
</ContentView>
```

It has a ```PathThickness``` bindable property if you want some big thick fluffy gradient (here ```PathThickness="6"```):

<img src="__Docs__/glenn_thick.png" width="300" />


Its input data is a ```SessionMapInfo```, which is basically a list of Gps points with session infos, the ```ISessionDisplayablePoint```.

```csharp
public static readonly BindableProperty SessionMapInfoProperty = BindableProperty.Create(
typeof(SessionMapInfo),
    nameof(SessionMapInfo),
    typeof(SessionMap),
    propertyChanged: SessionMapInfoChanged);
```

```csharp
public class SessionMapInfo
{
    public IReadOnlyList<SessionDisplayablePoint> SessionPoints { get; }

    public Bounds Region { get; }

    public int TotalDurationInSeconds { get; }
}

public interface ISessionDisplayablePoint
{
    TimeSpan Time { get; }

    Color MapPointColor { get; }

    int? Altitude { get; }

    int? HeartRate { get; }

    double? Speed { get; }

    LatLong Position { get; }

    bool HasMarker { get; }

    string Label { get; }

    int? Distance { get; }
}
```

In the cas of out Run Away! app, the ```SessionMapInfo``` is built in the ```ActivityPageViewModel``` as follows:

1. The selected tcx activity is retrieved from the ```ITcxActivityService```
2. The Tcx ```TrackPoint``` list is converted to domain ```ActivityPoint``` list
3. We specify some parameters like the number of markers and the distance label interval
4. Then ```SessionMap.Create``` factory is called and will compute speed, color from the ```EffortComputer```, distance, etc...

## Final Thanks

Thanks again to David, Steven, and Glenn!

Huge thanks to [Matthew Leibowitz](https://twitter.com/mattleibow) who unleashed the infinite power of ```Xamarin.Forms``` with ```SkiaSharp```.
