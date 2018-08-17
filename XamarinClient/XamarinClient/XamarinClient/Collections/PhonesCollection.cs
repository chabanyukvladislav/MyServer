using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.AspNetCore.SignalR.Client;
using XamarinClient.Key;
using XamarinClient.Models;
using XamarinClient.Services;

namespace XamarinClient.Collections
{
    public class PhonesCollection : INotifyCollectionChanged
    {
        private const string HubAddress = "http://localhost/Notification/";
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

        private PhonesCollection()
        {
            _dataStore = DataStore.GetDataStore;
            _hubConnection = new HubConnectionBuilder().WithUrl(HubAddress).Build();
            StartHub();
            MyKey.OnKeyChanged += UpdateCollection;
            UpdateCollection();
        }

        private async void UpdateCollection()
        {
            Peoples = await _dataStore.GetItemsAsync();
            OnCollectionChanged(NotifyCollectionChangedAction.Reset, null);
        }

        private async void StartHub()
        {
            await _hubConnection.StartAsync();
            _hubConnection.On<Guid>("Add", (value) =>
            {
                if (Peoples.Any(people => people.Id == value)) return;
                People val = GetPeople(value);
                Peoples.Add(val);
                OnCollectionChanged(NotifyCollectionChangedAction.Add, val);
            });
            _hubConnection.On<Guid>("Edit", (value) =>
            {
                People val = GetPeople(value);
                People local = Peoples.FirstOrDefault(people => people.Id == value);
                if (local == null || local.Equals(val)) return;
                Peoples.Remove(local);
                OnCollectionChanged(NotifyCollectionChangedAction.Remove, local);
                Peoples.Add(val);
                OnCollectionChanged(NotifyCollectionChangedAction.Add, val);
            });
            _hubConnection.On<Guid>("Delete", (value) =>
            {
                People val = Peoples.Find(people => people.Id == value);
                if (val == null) return;
                Peoples.Remove(val);
                OnCollectionChanged(NotifyCollectionChangedAction.Remove, val);
            });
        }

        public List<People> GetCollection()
        {
            return Peoples;
        }

        public People GetPeople(Guid id)
        {
            return _dataStore.GetItemAsync(id).Result;
        }
        public async void AddPhone(People item)
        {
            await _dataStore.AddItemAsync(item);
        }
        public async void EditPhone(People item)
        {
            await _dataStore.UpdateItemAsync(item);
        }
        public async void RemovePhone(People item)
        {
            await _dataStore.DeleteItemAsync(item.Id);
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        protected void OnCollectionChanged(NotifyCollectionChangedAction action, People item)
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, item));
        }
    }
}