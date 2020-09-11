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