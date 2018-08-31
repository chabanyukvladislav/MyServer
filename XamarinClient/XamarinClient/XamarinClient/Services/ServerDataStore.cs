using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
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
        private const string UserServerAddress = "http://185.247.21.82:9090/api/account";
        //private const string UserServerAddress = "http://localhost:6881/api/account";
        private const string PeopleUserServerAddress = "http://185.247.21.82:9090/api/peopleuser";
        //private const string PeopleUserServerAddress = "http://localhost:6881/api/peopleuser";
        private const string MessageServerAddress = "http://185.247.21.82:9090/api/message";
        //private const string MessageServerAddress = "http://localhost:6881/api/message";
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

        public static ServerDataStore GetDataStore
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

        public async Task<List<KeyValuePair<string, string>>> GetUsers()
        {
            return await Task.Run(() =>
            {
                try
                {
                    _response = _client.GetAsync(UserServerAddress).Result;
                    List<KeyValuePair<string, string>> data = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(_response.Content.ReadAsStringAsync().Result,
                               new JsonSerializerSettings()
                               {
                                   Error =
                                       (sender, args) => { args.ErrorContext.Handled = true; }
                               }) ?? new List<KeyValuePair<string, string>>();
                    return data;
                }
                catch (Exception)
                {
                    return new List<KeyValuePair<string, string>>();
                }
            });
        }

        public async Task<List<PeopleUser>> GetPeopleUsers()
        {
            return await Task.Run(() =>
            {
                try
                {
                    _response = _client.GetAsync(PeopleUserServerAddress).Result;
                    List<PeopleUser> data = JsonConvert.DeserializeObject<List<PeopleUser>>(_response.Content.ReadAsStringAsync().Result,
                                                                  new JsonSerializerSettings()
                                                                  {
                                                                      Error =
                                                                          (sender, args) => { args.ErrorContext.Handled = true; }
                                                                  }) ?? new List<PeopleUser>();
                    return data;
                }
                catch (Exception)
                {
                    return new List<PeopleUser>();
                }
            });
        }

        public async Task<PeopleUser> GetUser(string id)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _response = _client.GetAsync(PeopleUserServerAddress + '/' + id).Result;
                    PeopleUser data = JsonConvert.DeserializeObject<PeopleUser>(_response.Content.ReadAsStringAsync().Result,
                                                                  new JsonSerializerSettings()
                                                                  {
                                                                      Error =
                                                                          (sender, args) => { args.ErrorContext.Handled = true; }
                                                                  }) ?? new PeopleUser();
                    return data;
                }
                catch (Exception)
                {
                    return new PeopleUser();
                }
            });
        }

        public async void AddPeopleUser(KeyValuePair<string, string> user)
        {
            await Task.Run(() =>
            {
                try
                {
                    PeopleUser us = new PeopleUser() { Id = user.Key, Nick = user.Value };
                    string json = JsonConvert.SerializeObject(us);
                    int index = json.IndexOf("null", StringComparison.Ordinal);
                    while (index != -1)
                    {
                        int i = json.Remove(index - 3).LastIndexOf("\"", StringComparison.Ordinal);
                        string str1 = json.Remove(0, i + 1);
                        string str2 = str1.Remove(str1.IndexOf("null", StringComparison.Ordinal) - 2);
                        json = json.Remove(index - 3 - str2.Length, 8 + str2.Length);
                        index = json.IndexOf("null", StringComparison.Ordinal);
                    }

                    if (json[json.Length - 1] == ',')
                        json = json.Remove(json.Length - 1) + '}';
                    StringContent data = new StringContent(json, Encoding.UTF8, "application/json");
                    _response = _client.PostAsync(PeopleUserServerAddress, data).Result;
                }
                catch (Exception) { }
            });
        }

        public async Task<List<Messager>> GetMessages()
        {
            return await Task.Run(() =>
            {
                try
                {
                    _response = _client.GetAsync(MessageServerAddress).Result;
                    List<Messager> data = JsonConvert.DeserializeObject<List<Messager>>(_response.Content.ReadAsStringAsync().Result,
                                            new JsonSerializerSettings()
                                            {
                                                Error =
                                                    (sender, args) => { args.ErrorContext.Handled = true; }
                                            }) ?? new List<Messager>();
                    return data;
                }
                catch (Exception)
                {
                    return new List<Messager>();
                }
            });
        }

        //public async Task<List<Messager>> GetMessages()
        //{
        //    return await Task.Run(() =>
        //    {
        //        try
        //        {
        //            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
        //            RSAParameters param = rsa.ExportParameters(false);
        //            string address = MessageServerAddress + "?modul=" + InString(param.Modulus) +
        //                             "&exponent=" + InString(param.Exponent);
        //            _response = _client.GetAsync(address).Result;
        //            byte[] arr = _response.Content.ReadAsByteArrayAsync().Result;
        //            byte[] res = rsa.Decrypt(arr, false);
        //            BinaryFormatter formatter = new BinaryFormatter();
        //            MemoryStream stream = new MemoryStream();
        //            stream.Write(res, 0, res.Length);
        //            List<Messager> list = formatter.Deserialize(stream) as List<Messager>;
        //            List<Messager> data = JsonConvert.DeserializeObject<List<Messager>>(_response.Content.ReadAsStringAsync().Result,
        //                                      new JsonSerializerSettings()
        //                                      {
        //                                          Error =
        //                                              (sender, args) => { args.ErrorContext.Handled = true; }
        //                                      }) ?? new List<Messager>();
        //            return data;
        //        }
        //        catch (Exception)
        //        {
        //            return new List<Messager>();
        //        }
        //    });
        //}

        //private string InString(byte[] val)
        //{
        //    string str = "";
        //    foreach (byte b in val)
        //    {
        //        str += b + " ";
        //    }

        //    return str;
        //}

        public async Task<Messager> GetMessage(Guid id)
        {
            return await Task.Run(() =>
            {
                try
                {
                    _response = _client.GetAsync(MessageServerAddress + '/' + id).Result;
                    return JsonConvert.DeserializeObject<Messager>(_response.Content.ReadAsStringAsync().Result, new JsonSerializerSettings()
                    {
                        Error =
                            (sender, args) => { args.ErrorContext.Handled = true; }
                    }) ?? new Messager();
                }
                catch (Exception)
                {
                    return new Messager();
                }
            });
        }

        public async void SendMessage(string message, string userId)
        {
            await Task.Run(() =>
            {
                try
                {
                    Messager mes = new Messager() { Sms = message, ToId = userId };
                    string json = JsonConvert.SerializeObject(mes);
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
                    _response = _client.PostAsync(MessageServerAddress, data).Result;
                }
                catch (Exception) { }
            });
        }

        public event Action OnDisconnect;
        private event Action UserIdChanged;
    }
}