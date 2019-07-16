using System.IO;
using System.Linq;
using System.Reflection;

namespace SkiaSharpnado.Maps.Images
{
    internal static class Embedded
    {
        private static readonly Assembly Assembly;

        private static readonly string[] Resources;

        static Embedded()
        {
            Assembly = typeof(Embedded).GetTypeInfo().Assembly;
            Resources = Assembly.GetManifestResourceNames();
        }

        public static Stream Load(string name)
        {
            name = $".Images.{name}";
            name = Resources.FirstOrDefault(n => n.EndsWith(name));

            Stream stream = null;
            if (name != null)
            {
                stream = Assembly.GetManifestResourceStream(name);
            }

            return stream;
        }
    }
}