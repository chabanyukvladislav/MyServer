using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using Xamarin.Forms;
using XamarinClient.Models;
using XamarinClient.Services;
using XamarinClient.Views;

namespace XamarinClient.ViewModels
{
    internal class OAuthViewModel
    {
        private const string ServerAddress = "http://185.247.21.82:9090/api/account";
        //private const string ServerAddress = "http://localhost:6881/api/account";
        private readonly HttpClient _client;
        private readonly ServerDataStore _serverDataStore;

        public OAuthViewModel()
        {
            _serverDataStore = (ServerDataStore)ServerDataStore.GetDataStore;
            _client = new HttpClient();
            WebView wv = new WebView()
            {
                Source =
                    @"https://itstep1511.eu.auth0.com/authorize?response_type=code&client_id=zIrxMH5oEejRE3eu7AOBajsk5Ad4BXdz&redirect_uri=https://itstep1511.eu.auth0.com/mobile&scope=openid%20profile%20email",
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand
            };
            wv.Navigated += Authorised;
            if (!(Application.Current.MainPage is NavigationPage p)) return;
            StackLayout page = p.CurrentPage.FindByName<StackLayout>("OAuthContent");
            page.Children.Add(wv);
        }

        private async void Authorised(object sender, WebNavigatedEventArgs e)
        {
            if (e.Result != WebNavigationResult.Success)
            {
                MainPage();
                return;
            }
            int index = e.Url.IndexOf("code", StringComparison.Ordinal);
            string code = e.Url.Remove(0, index + 5);
            if (code[code.Length - 1] == '#')
                code = code.Remove(code.Length - 1);
            StringContent data = new StringContent("{\"code\": \"" + code + "\", \"type\": \"native\"}");
            HttpResponseMessage response = await _client.PostAsync(ServerAddress, data);
            if (!response.IsSuccessStatusCode)
            {
                MainPage();
                return;
            }

            string token = await response.Content.ReadAsStringAsync();
            JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            if (!(jwtSecurityTokenHandler.ReadToken(token) is JwtSecurityToken jwtSecurityToken))
            {
                MainPage();
                return;
            }
            User user = GetUser(jwtSecurityToken);
            Application.Current.Properties.Add("token", user.UserId);
            _serverDataStore.UserId = user.UserId;
            Application.Current.MainPage = new NavigationPage(new ItemsPage());
        }

        private async void MainPage()
        {
            await Application.Current.MainPage.Navigation.PopAsync();
        }

        private static User GetUser(JwtSecurityToken jwtSecurityToken)
        {
            string sub = jwtSecurityToken.Claims.FirstOrDefault(el => el.Type == "UserId")?.Value;
            string name = jwtSecurityToken.Claims.FirstOrDefault(el => el.Type == "Name")?.Value;
            string nickname = jwtSecurityToken.Claims.FirstOrDefault(el => el.Type == "Nickname")?.Value;
            string picture = jwtSecurityToken.Claims.FirstOrDefault(el => el.Type == "Picture")?.Value;
            User user = new User() { UserId = sub, Name = name, Nickname = nickname, Picture = picture };
            return user;
        }
    }
}
