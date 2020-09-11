using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace PCController
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();


            BindingContext = new ControllerService(new SyncLocalStorageService());
        }
    }
}
