using System.Windows.Input;
using Xamarin.Forms;
using XamarinClient.Key;
using XamarinClient.Models;
using XamarinClient.Services;
using XamarinClient.Views;

namespace XamarinClient.ViewModels
{
    class LoginViewModel
    {
        private readonly IDataStore _dataStore;

        public User Item { get; set; }
        public ICommand Login { get; }

        public LoginViewModel()
        {
            _dataStore = DataStore.GetDataStore;
            Login = new Command(ExecuteLogin);
            Item = new User();
        }

        private async void ExecuteLogin()
        {
            if (string.IsNullOrWhiteSpace(Item.Login) || string.IsNullOrWhiteSpace(Item.Password))
                return;
            await _dataStore.LoginAsync(Item);
            if (MyKey.IsEnable)
                Application.Current.MainPage = new NavigationPage(new ItemsPage());
        }
    }
}
