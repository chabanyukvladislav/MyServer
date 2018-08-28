using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinClient.ViewModels;

namespace XamarinClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AccountPage : ContentPage
    {
        public AccountPage()
        {
            InitializeComponent();
            BindingContext = new AccountViewModel();
            Appearing += Appeared;
        }

        private void Appeared(object sender, EventArgs e)
        {
            if (Application.Current.Properties.ContainsKey("token"))
                Application.Current.MainPage = new NavigationPage(new ItemsPage());
        }
    }
}