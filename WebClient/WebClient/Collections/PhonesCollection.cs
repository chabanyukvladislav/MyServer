using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using WebClient.Hubs;
using WebClient.Models;
using WebClient.Services;

namespace WebClient.Collections
{
    public class PhonesCollection
    {
        private const string HubAddress = "http://185.247.21.82:9090/Notification/";
        //private const string HubAddress = "http://localhost:6881/Notification/";
        private readonly IHubContext<UpdateHub> _myHub;
        private readonly HubConnection _hubConnection;
        private readonly IDataStore _dataStore;
        private readonly string _key;

        public static Dictionary<string, PhonesCollection> PeoplesListDictionary { get; } = new Dictionary<string, PhonesCollection>();

        private List<People> Peoples { get; set; }

        public bool IsChanged { get; set; }

        public PhonesCollection(string userId, IHubContext<UpdateHub> hub)
        {
            _key = userId;
            _myHub = hub;
            IsChanged = false;
            Peoples = new List<People>();
            _dataStore = DataStore.GetDataStore;
            _hubConnection = new HubConnectionBuilder().WithUrl(HubAddress).Build();
            StartHub();
            UpdateCollection();
        }

        private async void UpdateCollection()
        {
            IsChanged = true;
            List<People> list = await _dataStore.GetItemsAsync(_key);
            Peoples = list ?? new List<People>();
            IsChanged = false;
            Update(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private async void StartHub()
        {
            try
            {
                await _hubConnection.StartAsync();
                _hubConnection.On<Guid>("Add", async (value) =>
                {
                    if (Peoples.Any(people => people.Id == value))
                    {
                        IsChanged = false;
                        return;
                    }
                    People val = await GetPeople(value);
                    if (val == null)
                    {
                        IsChanged = false;
                        return;
                    }
                    Peoples.Add(val);
                    IsChanged = false;
                    Update(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, val));
                });
                _hubConnection.On<Guid>("Edit", async (value) =>
                {
                    People val = await GetPeople(value);
                    if (val == null)
                    {
                        IsChanged = false;
                        return;
                    }
                    People local = Peoples.FirstOrDefault(people => people.Id == value);
                    if (local == null || local.Equals(val))
                    {
                        IsChanged = false;
                        return;
                    }
                    Peoples.Remove(local);
                    Update(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, local));
                    Peoples.Add(val);
                    IsChanged = false;
                    Update(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, val));
                });
                _hubConnection.On<Guid>("Delete", (value) =>
                {
                    People val = Peoples.Find(people => people.Id == value);
                    if (val == null)
                    {
                        IsChanged = false;
                        return;
                    }
                    Peoples.Remove(val);
                    IsChanged = false;
                    Update(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, val));
                });
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private void Update(object sender, NotifyCollectionChangedEventArgs e)
        {
            _myHub.Clients.All.SendAsync("Update");
        }

        public List<People> GetCollection()
        {
            return Peoples;
        }

        public async Task<People> GetPeople(Guid id)
        {
            return await _dataStore.GetItemAsync(id, _key);
        }
        public async void AddPhone(People item)
        {
            IsChanged = true;
            await _dataStore.AddItemAsync(item, _key);
        }
        public async void EditPhone(People item)
        {
            IsChanged = true;
            await _dataStore.UpdateItemAsync(item, _key);
        }
        public async void RemovePhone(Guid id)
        {
            IsChanged = true;
            await _dataStore.DeleteItemAsync(id, _key);
        }
    }
}