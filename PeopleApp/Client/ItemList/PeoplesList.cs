using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Client.Key;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PeopleApp.Models;

namespace Client.ItemList
{
    public class PeoplesList
    {
        private string _serverAddress = "http://localhost:6881/api/peoples/" + MyKey.Key;
        private static readonly object Locker = new object();
        private static PeoplesList _peoplesList;

        public List<People> Peoples { get; }

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
            MyKey.OnKeyChanged += ChangeKey;
            HttpClient client = new HttpClient();
            HttpResponseMessage response = client.GetAsync(_serverAddress).Result;
            List<People> data = JsonConvert.DeserializeObject<List<People>>(response.Content.ReadAsStringAsync().Result);
            Peoples = data ?? new List<People>();
        }
        
        private void ChangeKey()
        {
            _serverAddress = "http://localhost:6881/api/peoples/" + MyKey.Key;
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
            HttpResponseMessage response = client.PostAsync(_serverAddress, data).Result;
            if (!response.IsSuccessStatusCode)
                return false;
            Peoples.Add(value);
            OnPeopleAdd?.Invoke(value);
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
            HttpResponseMessage response = client.PostAsync(_serverAddress, data).Result;
            if (!response.IsSuccessStatusCode)
                return false;
            People val = Peoples.FirstOrDefault(people => people.Id == value.Id);
            if (val != null)
                Peoples.Remove(val);
            Peoples.Add(value);
            return true;
        }

        public bool DeletePeople(Guid value)
        {
            HttpClient client = new HttpClient();
            StringContent data = new StringContent("{\"id\": \"" + value + "\"}}", Encoding.UTF8, "application/json");
            HttpResponseMessage response = client.PutAsync(_serverAddress, data).Result;
            if (!response.IsSuccessStatusCode)
                return false;
            People val = Peoples.FirstOrDefault(people => people.Id == value);
            if (val == null)
                return true;
            Peoples.Remove(val);
            OnPeopleDelete?.Invoke(value);
            return true;
        }

        public event Action<People> OnPeopleAdd;
        public event Action<Guid> OnPeopleDelete;
    }
}
