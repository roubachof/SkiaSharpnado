using System;
using Xamarin.Forms;

namespace Sample
{
    public static class ResourcesHelper
    {
        public static Color GetResourceColor(string key)
        {
            if (Application.Current.Resources.TryGetValue(key, out var value))
            {
                return (Color)value;
            }

            throw new InvalidOperationException($"key {key} not found in the resource dictionary");
        }
    }
}
