using System;
using System.Collections.Generic;
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
            WebView wv = new WebView()
            {
                Source =
                    @"https://itstep1511.eu.auth0.com/authorize?response_type=code&client_id=zIrxMH5oEejRE3eu7AOBajsk5Ad4BXdz&redirect_uri=https://itstep1511.eu.auth0.com/mobile&scope=openid%20profile%20email",
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
            NavigationPage p = Application.Current.MainPage as NavigationPage;
            StackLayout sl = p.CurrentPage.FindByName<StackLayout>("AccountPageContent");
            sl.Children.Clear();
            sl.Children.Add(wv);
            //Auth0Client client = new Auth0Client(new Auth0ClientOptions()
            //{
            //    Domain = "itstep1511.eu.auth0.com",
            //    ClientId = "zIrxMH5oEejRE3eu7AOBajsk5Ad4BXdz",
            //    ClientSecret = "zcVkBGepy2aIzBjfncRz5OLMkFbJc42zB4Efje3nqNz1awGo2wVSz9sMS_f_LSDf",
            //    Scope = "openid%20profile%20email"
            //});
            //LoginResult loginResult = await client.LoginAsync();
        }
    }
}
