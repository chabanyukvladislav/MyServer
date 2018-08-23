using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Input;
using Auth0.OidcClient;
using IdentityModel.OidcClient;
using Xamarin.Forms;

namespace XamarinClient.ViewModels
{
    class AccountViewModel
    {
        public ICommand LoginAs { get; }

        public AccountViewModel()
        {
            LoginAs = new Command(ExecuteLoginAs);
        }

        private async void ExecuteLoginAs()
        {
            Auth0Client client = new Auth0Client(new Auth0ClientOptions()
            {
                Domain = "vlad191100.eu.auth0.com",
                ClientId = "dhVeAtvG04Q5NkaN54iwYJJHDYD1vC9c"
            });
            LoginResult loginResult = await client.LoginAsync();
        }
    }
}
