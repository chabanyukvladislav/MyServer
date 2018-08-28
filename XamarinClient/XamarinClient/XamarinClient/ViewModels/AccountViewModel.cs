using System.Windows.Input;
using Xamarin.Forms;
using XamarinClient.Views;

namespace XamarinClient.ViewModels
{
    internal class AccountViewModel
    {
        public ICommand LoginAs { get; }

        public AccountViewModel()
        {
            LoginAs = new Command(ExecuteLoginAs);
        }

        private async void ExecuteLoginAs()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new OAuthPage());
        }
    }
}
