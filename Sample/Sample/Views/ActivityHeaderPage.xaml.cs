using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Sample.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ActivityHeaderPage : ContentPage
    {
        public ActivityHeaderPage()
        {
            InitializeComponent();
        }

        private void ListViewOnItemTapped(object sender, ItemTappedEventArgs e)
        {
            ListView.SelectedItem = null;
        }
    }
}