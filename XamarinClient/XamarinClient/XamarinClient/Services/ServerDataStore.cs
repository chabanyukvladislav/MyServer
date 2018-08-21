using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using XamarinClient.Collections;
using XamarinClient.Enum;
using XamarinClient.Models;

namespace XamarinClient.Services
{
    public class ServerDataStore : IDataStore
    {
        private const string ServerAddress = "http://192.168.1.19:1919/api/peoples";
        private static readonly object Locker = new object();
        private static ServerDataStore _dataStore;
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
                            _dataStore = new ServerDataStore();
                        }
                    }
                }
                return _dataStore;
            }
        }

        private ServerDataStore()
        {
            _client = new HttpClient {Timeout = TimeSpan.FromSeconds(10)};
        }

        public bool IsConnect
        {
            get
            {
                try
                {
                    return _client.GetAsync(ServerAddress).Result.IsSuccessStatusCode;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        public async Task<bool> Synchronized()
        {
            return await Task.Run(async () =>
            {
                try
                {
                    foreach (LocalAction value in Synchronizer.GetItems())
                    {
                        switch (value.Type)
                        {
                            case TypeOfActions.Add:
                                await AddItemAsync(value.People);
                                break;
                            case TypeOfActions.Edit:
                                await UpdateItemAsync(value.People);
                                break;
                            case TypeOfActions.Delete:
                                await DeleteItemAsync(value.People.Id);
                                break;
                            default:
                                return false;
                        }
                    }
                    Synchronizer.Clear();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            });
        }

        public async Task<bool> AddItemAsync(People item)
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
                    if (!_response.IsSuccessStatusCode)
                        return false;
                    return true;
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
                    if (!_response.IsSuccessStatusCode)
                        return false;
                    return true;
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
                    if (!_response.IsSuccessStatusCode)
                        return false;
                    return true;
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
    }
}