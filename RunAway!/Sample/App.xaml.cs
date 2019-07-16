using System;

using Prism;
using Prism.DryIoc;
using Prism.Ioc;

using Sample.Domain;
using Sample.ViewModels;
using Sample.Views;

using SkiaSharpnado.SkiaSharp;

using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Sample
{
    public partial class App : PrismApplication
    {
        public App() 
            : this(null)
        {
        }

        public App(IPlatformInitializer initializer)
            : base(initializer)
        {
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.Register<ITcxActivityService, TcxActivityService>();

            containerRegistry.RegisterForNavigation<NavigationPage>();
            containerRegistry.RegisterForNavigation<ActivityHeaderPage, ActivityHeaderPageViewModel>();
            containerRegistry.RegisterForNavigation<ActivityPage, ActivityPageViewModel>();
        }

        protected override async void OnInitialized()
        {
            Xamarin.Forms.Internals.Log.Listeners.Add(new DelegateLogListener((arg1, arg2) => Console.WriteLine($"{arg1}: {arg2}")));

            InitializeComponent();

            SkiaHelper.Initialize((float)DeviceDisplay.MainDisplayInfo.Density);

            await NavigationService.NavigateAsync("NavigationPage/ActivityHeaderPage");
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
