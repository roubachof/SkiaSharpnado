using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Prism.Navigation;

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