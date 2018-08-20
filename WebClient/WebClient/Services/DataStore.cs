using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WebClient.Models;

namespace WebClient.Services
{
    public class DataStore : IDataStore
    {
        private const string ServerAddress = "http://192.168.1.19:1919/api/peoples";
        private static readonly object Locker = new object();
        private static DataStore _dataStore;
        private readonly HttpClient _client;
        private HttpResponseMessage _response;

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
            _client = new HttpClient();
        }

        public async Task<bool> AddItemAsync(People item)
        {
            return await Task.Run(() =>
            {
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
                _response = _client.PostAsync(ServerAddress, data).Result;
                if (!_response.IsSuccessStatusCode)
                    return false;
                return true;
            });
        }

        public async Task<bool> UpdateItemAsync(People item)
        {
            return await Task.Run(() =>
            {
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
                _response = _client.PostAsync(ServerAddress, data).Result;
                if (!_response.IsSuccessStatusCode)
                    return false;
                return true;
            });
        }

        public async Task<bool> DeleteItemAsync(Guid id)
        {
            return await Task.Run(() =>
            {
                _response = _client.DeleteAsync(ServerAddress + '/' + id).Result;
                if (!_response.IsSuccessStatusCode)
                    return false;
                return true;
            });
        }

        public async Task<List<People>> GetItemsAsync()
        {
            return await Task.Run(() =>
            {
                _response = _client.GetAsync(ServerAddress).Result;
                List<People> data = JsonConvert.DeserializeObject<List<People>>(
                    _response.Content.ReadAsStringAsync().Result, new JsonSerializerSettings()
                    {
                        Error =
                            (sender, args) => { args.ErrorContext.Handled = true; }
                    });
                return data;
            });
        }

        public async Task<People> GetItemAsync(Guid id)
        {
            return await Task.Run(() =>
            {
                _response = _client.GetAsync(ServerAddress + '/' + id).Result;
                return JsonConvert.DeserializeObject<People>(_response.Content.ReadAsStringAsync().Result, new JsonSerializerSettings()
                {
                    Error =
                        (sender, args) => { args.ErrorContext.Handled = true; }
                }) ?? new People();
            });
        }
    }
}