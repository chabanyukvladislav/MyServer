using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using WebClient.Models;
using WebClient.Services;

namespace WebClient.Collections
{
    public class PhonesCollection : INotifyCollectionChanged
    {
        private const string HubAddress = "http://vlad191100.server.com/Notification/";
        private static readonly object Locker = new object();
        private readonly HubConnection _hubConnection;
        private readonly IDataStore _dataStore;
        private static PhonesCollection _phonesCollection;

        public static PhonesCollection GetPhonesCollection
        {
            get
            {
                if (_phonesCollection == null)
                {
                    lock (Locker)
                    {
                        if (_phonesCollection == null)
                        {
                            _phonesCollection = new PhonesCollection();
                        }
                    }
                }

                return _phonesCollection;
            }
        }
        private List<People> Peoples { get; set; }

        public bool IsChanged { get; set; }

        private PhonesCollection()
        {
            IsChanged = false;
            Peoples = new List<People>();
            _dataStore = DataStore.GetDataStore;
            _hubConnection = new HubConnectionBuilder().WithUrl(HubAddress).Build();
            StartHub();
            UpdateCollection();
        }

        private void UpdateCollection()
        {
            Task<List<People>> task = _dataStore.GetItemsAsync();
            task.Wait();
            Peoples = task.Result;
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private async void StartHub()
        {
            await _hubConnection.StartAsync();
            _hubConnection.On<Guid>("Add", (value) =>
            {
                if (Peoples.Any(people => people.Id == value)) return;
                People val = GetPeople(value).Result;
                Peoples.Add(val);
                IsChanged = false;
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, val));
            });
            _hubConnection.On<Guid>("Edit", (value) =>
            {
                People val = GetPeople(value).Result;
                People local = Peoples.FirstOrDefault(people => people.Id == value);
                if (local == null || local.Equals(val)) return;
                Peoples.Remove(local);
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, local));
                Peoples.Add(val);
                IsChanged = false;
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, val));
            });
            _hubConnection.On<Guid>("Delete", (value) =>
            {
                People val = Peoples.Find(people => people.Id == value);
                if (val == null) return;
                Peoples.Remove(val);
                IsChanged = false;
                CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, val));
            });
        }

        public List<People> GetCollection()
        {
            return Peoples;
        }

        public async Task<People> GetPeople(Guid id)
        {
            return await _dataStore.GetItemAsync(id);
        }
        public async void AddPhone(People item)
        {
            IsChanged = true;
            await _dataStore.AddItemAsync(item);
        }
        public async void EditPhone(People item)
        {
            IsChanged = true;
            await _dataStore.UpdateItemAsync(item);
        }
        public async void RemovePhone(Guid id)
        {
            IsChanged = true;
            await _dataStore.DeleteItemAsync(id);
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
    }
}