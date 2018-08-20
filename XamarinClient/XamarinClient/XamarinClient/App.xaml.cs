using Xamarin.Forms;
using XamarinClient.Views;

namespace XamarinClient
{
    public partial class App
    {
        public App()
        {
            InitializeComponent();
            Current.MainPage = new NavigationPage(new ItemsPage());
        }
    }
}
