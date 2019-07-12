using SkiaSharp;

namespace SkiaSharpnado.Maps.Presentation.Views.SessionMap
{
    public abstract class ASinglePointShape : AShape, ISinglePointShape
    {
        public SKPoint Point { get; protected set; }
    }
}