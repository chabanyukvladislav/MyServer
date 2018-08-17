using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using XamarinClient.Key;
using XamarinClient.Models;

[assembly: Xamarin.Forms.Dependency(typeof(XamarinClient.Services.DataStore))]
namespace XamarinClient.Services
{
    public class DataStore : IDataStore
    {
        private const string ServerAddress = "http://localhost/api/peoples";
        private const string ServerAccountAddress = "http://localhost/api/account";
        private string _key = "?token=" + MyKey.Key;
        private static readonly object Locker = new object();
        private static DataStore _dataStore;

        public static IDataStore GetDataStore
        {
            get
            {
                if (_dataStore == null)
                {
                    lock (Locker)
                    {
                        if (_dataStore == null)
                        {
                            _dataStore = new DataStore();
                        }
                    }
                }
                return _dataStore;
            }
        }

        private DataStore()
        {
            MyKey.OnKeyChanged += KeyChanged;
        }

        private void KeyChanged()
        {
            _key = "?token=" + MyKey.Key;
        }

        public async Task LoginAsync(User user)
        {
            await Task.Run(() =>
            {
                try
                {
                    HttpClient client = new HttpClient();
                    string json = JsonConvert.SerializeObject(user);
                    StringContent data = new StringContent(json, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = client.PostAsync(ServerAccountAddress, data).Result;
                    string key = response.Content.ReadAsStringAsync().Result.Trim('"');
                    MyKey.Key = Guid.Parse(key);
                }
                catch (Exception)
                {
                    // ignored
                }
            });
        }

        public async Task<bool> AddItemAsync(People item)
        {
            return await Task.Run(() =>
            {
                HttpClient client = new HttpClient();
                string json = JsonConvert.SerializeObject(item);
                int index = json.IndexOf("null", StringComparison.Ordinal);
                while (index != -1)
                {
                    int i = json.Remove(index - 3).LastIndexOf("\"", StringComparison.Ordinal);
                    string str1 = json.Remove(0, i + 1);
                    string str2 = str1.Remove(str1.IndexOf("null", StringComparison.Ordinal) - 2);
                    json = json.Remove(index - 3 - str2.Length, 8 + str2.Length);
                    index = json.IndexOf("null", StringComparison.Ordinal);
                }
                StringContent data = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = client.PostAsync(ServerAddress + _key, data).Result;
                if (!response.IsSuccessStatusCode)
                    return false;
                return true;
            });
        }

        public async Task<bool> UpdateItemAsync(People item)
        {
            return await Task.Run(() =>
            {
                HttpClient client = new HttpClient();
                string json = JsonConvert.SerializeObject(item);
                int index = json.IndexOf("null", StringComparison.Ordinal);
                while (index != -1)
                {
                    int i = json.Remove(index - 3).LastIndexOf("\"", StringComparison.Ordinal);
                    string str1 = json.Remove(0, i + 1);
                    string str2 = str1.Remove(str1.IndexOf("null", StringComparison.Ordinal) - 2);
                    json = json.Remove(index - 3 - str2.Length, 8 + str2.Length);
                    index = json.IndexOf("null", StringComparison.Ordinal);
                }
                StringContent data = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage response = client.PostAsync(ServerAddress + _key, data).Result;
                if (!response.IsSuccessStatusCode)
                    return false;
                return true;
            });
        }

        public async Task<bool> DeleteItemAsync(Guid id)
        {
            return await Task.Run(() =>
            {
                HttpClient client = new HttpClient();
                HttpResponseMessage response = client.DeleteAsync(ServerAddress + '/' + id + _key).Result;
                if (!response.IsSuccessStatusCode)
                    return false;
                return true;
            });
        }

        public async Task<List<People>> GetItemsAsync()
        {
            return await Task.Run(() =>
            {
                HttpClient client = new HttpClient();
                HttpResponseMessage response = client.GetAsync(ServerAddress + _key).Result;
                List<People> data = JsonConvert.DeserializeObject<List<People>>(
                    response.Content.ReadAsStringAsync().Result, new JsonSerializerSettings()
                    {
                        Error =
                            (sender, args) => { args.ErrorContext.Handled = true; }
                    });
                return data ?? new List<People>();
            });
        }

        public async Task<People> GetItemAsync(Guid id)
        {
            return await Task.Run(() =>
            {
                HttpClient client = new HttpClient();
                HttpResponseMessage response = client.GetAsync(ServerAddress + '/' + id + _key).Result;
                return JsonConvert.DeserializeObject<People>(response.Content.ReadAsStringAsync().Result, new JsonSerializerSettings()
                {
                    Error =
                        (sender, args) => { args.ErrorContext.Handled = true; }
                }) ?? new People();
            });
        }
    }
}