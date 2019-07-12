using SkiaSharp;

namespace SkiaSharpnado.Maps.Presentation.Views.SessionMap
{
    public interface ISinglePointShape : IShape
    {
        SKPoint Point { get; }
    }
}