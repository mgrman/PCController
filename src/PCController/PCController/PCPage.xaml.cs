using Xamarin.Forms;

namespace PCController
{
    public partial class PCPage : ContentPage
    {
        private int index;

        public PCPage()
        {
            InitializeComponent();

        }

        public int Index
        {
            get => index; set
            {
                index = value;

                BindingContext = new ControllerService(new SyncLocalStorageService(index));
            }
        }
    }
}