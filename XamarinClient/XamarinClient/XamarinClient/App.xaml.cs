using System;
using System.IO;
using SQLitePCL;
using XamarinClient.Views;
using Xamarin.Forms;
using XamarinClient.Key;

namespace XamarinClient
{
    public partial class App : Application
    {
        private const string FileName = "guid.txt";

        public App()
        {
            InitializeComponent();
            if (!FileLogin())
                MainPage = new NavigationPage(new LoginPage(GetPath(FileName)));
        }

        private bool FileLogin()
        {
            string path = GetPath(FileName);
            if (!File.Exists(path)) return false;
            StreamReader streamReader = File.OpenText(path);
            string guid = streamReader.ReadLine();
            string day = streamReader.ReadLine();
            streamReader.Close();
            if (!Guid.TryParse(guid, out Guid gui) || gui == Guid.Empty || !int.TryParse(day, out int d) || d + 7 < DateTime.Now.Day) return false;
            MyKey.Key = gui;
            MainPage = new NavigationPage(new ItemsPage());
            return true;
        }

        private string GetPath(string fileName)
        {
            string path;
            switch (Device.RuntimePlatform)
            {
                case Device.Android:
                    path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), fileName);
                    break;
                case Device.UWP:
                    path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), fileName);
                    break;
                case Device.iOS:
                    Batteries.Init();
                    path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "..", "Library", fileName);
                    break;
                default:
                    path = "";
                    break;
            }

            return path;
        }

        protected override void OnStart()
        {
            // Handle when your app starts
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
