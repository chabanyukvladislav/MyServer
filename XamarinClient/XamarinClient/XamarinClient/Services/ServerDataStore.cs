using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Xamarin.Forms;
using XamarinClient.Collections;
using XamarinClient.Enum;
using XamarinClient.Models;
using Encoding = System.Text.Encoding;

namespace XamarinClient.Services
{
    public class ServerDataStore : IDataStore
    {
        private const string ServerAddress = "http://185.247.21.82:9090/api/peoples";
        private static readonly object Locker = new object();
        private static ServerDataStore _dataStore;
        private readonly HttpClient _client;
        private HttpResponseMessage _response;
        private string _userId;

        public string UserId
        {
            get => _userId;
            set
            {
                _userId = value;
                UserIdChanged?.Invoke();
            }
        }

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
                            _dataStore = new ServerDataStore();
                        }
                    }
                }
                return _dataStore;
            }
        }

        private ServerDataStore()
        {
            _client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
            UserId = (string)Application.Current.Properties["token"];
            _client.DefaultRequestHeaders.Add("UserId", UserId);
            UserIdChanged += IdChanged;
        }

        private void IdChanged()
        {
            _client.DefaultRequestHeaders.Remove("UserId");
            _client.DefaultRequestHeaders.Add("UserId", UserId);
        }

        public bool IsConnect
        {
            get
            {
                try
                {
                    HttpResponseMessage response = _client.GetAsync(ServerAddress).Result;
                    return response.IsSuccessStatusCode;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public async Task TryConnect()
        {
            await Task.Run(() =>
            {
                while (IsConnect)
                {
                    Thread.Sleep(1000);
                }

                OnDisconnect?.Invoke();
            });
        }

        public async void Synchronized()
        {
            try
            {
                foreach (LocalAction value in Synchronizer.GetItems())
                {
                    switch (value.Type)
                    {
                        case TypeOfActions.Add:
                            await AddItemAsync(value.People);
                            Synchronizer.Clear();
                            break;
                        default:
                            return;
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        public async Task<bool> AddItemAsync(People item)
        {
            return await Task.Run(() =>
            {
                try
                {
                    item.Id = Guid.Empty;
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
                    return _response.IsSuccessStatusCode;
                }
                catch (Exception)
                {
                    return false;
                }
            });
        }

        public async Task<bool> UpdateItemAsync(People item)
        {
            return await Task.Run(() =>
            {
                try
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
                    return _response.IsSuccessStatusCode;
                }
                catch (Exception)
                {
                    return false;
                }

            });
        }

        public async Task<bool> DeleteItemAsync(Guid id)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _response = _client.DeleteAsync(ServerAddress + '/' + id).Result;
                    return _response.IsSuccessStatusCode;
                }
                catch (Exception)
                {
                    return false;
                }
            });
        }

        public async Task<List<People>> GetItemsAsync()
        {
            return await Task.Run(() =>
            {
                try
                {
                    _response = _client.GetAsync(ServerAddress).Result;
                    List<People> data = JsonConvert.DeserializeObject<List<People>>(
                        _response.Content.ReadAsStringAsync().Result, new JsonSerializerSettings()
                        {
                            Error =
                                (sender, args) => { args.ErrorContext.Handled = true; }
                        });
                    return data;
                }
                catch (Exception)
                {
                    return new List<People>();
                }
            });
        }

        public async Task<People> GetItemAsync(Guid id)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _response = _client.GetAsync(ServerAddress + '/' + id).Result;
                    return JsonConvert.DeserializeObject<People>(_response.Content.ReadAsStringAsync().Result, new JsonSerializerSettings()
                    {
                        Error =
                            (sender, args) => { args.ErrorContext.Handled = true; }
                    }) ?? new People();
                }
                catch (Exception)
                {
                    return new People();
                }
            });
        }


        public event Action OnDisconnect;
        private event Action UserIdChanged;
    }
}