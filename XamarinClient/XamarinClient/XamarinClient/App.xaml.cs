using Xamarin.Forms;
using XamarinClient.Views;

namespace XamarinClient
{
    public partial class App
    {
        public App()
        {
            InitializeComponent();
            AccountPage ap = new AccountPage();
            //Current.MainPage = new NavigationPage(new ItemsPage());
            Current.MainPage = new NavigationPage(ap);
        }
    }
}
