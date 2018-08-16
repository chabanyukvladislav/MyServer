using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.SignalR.Client;
using Newtonsoft.Json;
using WebClient.Key;
using WebClient.Models;

namespace WebClient.ItemList
{
    public class PeoplesList
    {
        private const string ServerAddress = "http://localhost/api/peoples";
        private const string HubAddress = "http://localhost/Notification/";
        private readonly string _key = "?token=" + MyKey.Key;
        private readonly HubConnection _hubConnection;
        private static readonly object Locker = new object();
        private static PeoplesList _peoplesList;

        private List<People> Peoples { get; set; }

        public static PeoplesList GetPeoplesList
        {
            get
            {
                if (_peoplesList == null)
                {
                    lock (Locker)
                    {
                        if (_peoplesList == null)
                            _peoplesList = new PeoplesList();
                    }
                }
                return _peoplesList;
            }
        }

        private PeoplesList()
        {
            _hubConnection = new HubConnectionBuilder().WithUrl(HubAddress).Build();
            StartHub();
            MyKey.OnKeyChanged += ChangeKey;
            LoadPeoples();
        }

        private void ChangeKey()
        {
            LoadPeoples();
        }

        private void LoadPeoples()
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = client.GetAsync(ServerAddress + _key).Result;
            List<People> data = JsonConvert.DeserializeObject<List<People>>(response.Content.ReadAsStringAsync().Result, new JsonSerializerSettings()
            {
                Error =
                    (sender, args) => { args.ErrorContext.Handled = true; }
            });
            Peoples = data ?? new List<People>();
        }

        private async void StartHub()
        {
            await _hubConnection.StartAsync();
            _hubConnection.On<Guid>("Add", (value) =>
            {
                if (Peoples.Any(people => people.Id == value))
                    return;
                People val = GetPeople(value);
                Peoples.Add(val);
                OnUpdate?.Invoke();
            });
            _hubConnection.On<Guid>("Edit", (value) =>
            {
                People val = GetPeople(value);
                People local = Peoples.FirstOrDefault(people => people.Id == value);
                if (local == null || local.Equals(val))
                    return;
                Peoples.Remove(local);
                Peoples.Add(val);
                OnUpdate?.Invoke();
            });
            _hubConnection.On<Guid>("Delete", (value) =>
            {
                People val = Peoples.Find(people => people.Id == value);
                if (val == null) return;
                Peoples.Remove(val);
                OnUpdate?.Invoke();
            });
        }

        public List<People> GetPeoples()
        {
            return Peoples;
        }

        public People GetPeople(Guid value)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = client.GetAsync(ServerAddress + '/' + value + _key).Result;
            return JsonConvert.DeserializeObject<People>(response.Content.ReadAsStringAsync().Result, new JsonSerializerSettings()
            {
                Error =
                    (sender, args) => { args.ErrorContext.Handled = true; }
            }) ?? new People();
        }

        public bool AddPeople(People value)
        {
            HttpClient client = new HttpClient();
            string json = JsonConvert.SerializeObject(value);
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
        }

        public bool EditPeople(People value)
        {
            HttpClient client = new HttpClient();
            string json = JsonConvert.SerializeObject(value);
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
        }

        public bool DeletePeople(Guid value)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = client.DeleteAsync(ServerAddress + '/' + value + _key).Result;
            if (!response.IsSuccessStatusCode)
                return false;
            return true;
        }

        public event Action OnUpdate;
    }
}
