using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

using SkiaSharpnado.Maps.Domain;

using Xamarin.Forms;

namespace Sample
{
    public class SportToIconConverter: IValueConverter
    {
        /// <summary>
        /// Returns true is value == null.
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value is Sport sport)
            {
                switch (sport)
                {
                    case Sport.Biking:
                        return (FontImageSource)GetResource("IconBikeSmall"); 
                    case Sport.Running:
                        return (FontImageSource)GetResource("IconRunSmall"); 
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static FontImageSource GetResource(string key)
        {
            if (Application.Current.Resources.TryGetValue(key, out var value))
            {
                return (FontImageSource)value;
            }

            throw new InvalidOperationException($"key {key} not found in the resource dictionary");
        }
    }
}
