using System;
using System.Collections.Generic;
using System.ComponentModel;
using Prism.Navigation;
using Sample.ViewModels;
using SkiaSharp;
using SkiaSharp.Views.Forms;
using SkiaSharpnado.Maps.Presentation.ViewModels.SessionMap;
using SkiaSharpnado.SkiaSharp;
using TouchTracking;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Sample.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ActivityPage : ContentPage, IDestructible
    {
        public ActivityPage()
        {
            InitializeComponent();
        }

        public void Destroy()
        {
            SessionMap.OnDestroy();
        }
    }
}