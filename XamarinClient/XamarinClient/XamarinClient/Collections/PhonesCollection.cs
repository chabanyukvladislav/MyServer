using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using XamarinClient.Models;
using XamarinClient.Services;

namespace XamarinClient.Collections
{
    public class PhonesCollection : INotifyCollectionChanged
    {
        private const string HubAddress = "http://vlad191100.server.com/Notification/";
        private static readonly object Locker = new object();
        private readonly HubConnection _hubConnection;
        private IDataStore _dataStore;
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
            _dataStore = ServerDataStore.GetDataStore;
            if (!_dataStore.IsConnect)
                _dataStore = DbDataStore.GetDataStore;
            _hubConnection = new HubConnectionBuilder().WithUrl(HubAddress).Build();
            StartHub();
            UpdateCollection();
        }

        private async void Connect()
        {
            await Task.Run(() =>
            {
                Thread.Sleep(5000);
                StartHub();
            });
        }

        private async void IsConnect()
        {
            await Task.Run(() =>
            {
                bool isDo = true;
                while (isDo)
                {
                    Thread.Sleep(5000);
                    if (_dataStore.IsConnect) continue;
                    isDo = false;
                    StartHub();
                }
            });
        }

        private async void UpdateCollection()
        {
            Peoples = await _dataStore.GetItemsAsync() ?? new List<People>();
            OnCollectionChanged(NotifyCollectionChangedAction.Reset, null);
        }

        private async void StartHub()
        {
            try
            {
                await _hubConnection.StartAsync();
                _dataStore = ServerDataStore.GetDataStore;
                await _dataStore.Synchronized();
                UpdateCollection();
                IsConnect();
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
            catch (Exception)
            {
                _dataStore = DbDataStore.GetDataStore;
                Connect();
            }
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
            if (!(_dataStore is DbDataStore)) return;
            DbDataStore dbds = (DbDataStore) _dataStore;
            People val = await dbds.GetItemAsync(item);
            if (val.Equals(new People())) return;
            Peoples.Add(val);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, val));
        }
        public async void EditPhone(People item)
        {
            await _dataStore.UpdateItemAsync(item);
            if (!(_dataStore is DbDataStore)) return;
            People val = Peoples.FirstOrDefault(el => el.Id == item.Id);
            Peoples.Remove(val);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, val));
            Peoples.Add(item);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }
        public async void RemovePhone(People item)
        {
            await _dataStore.DeleteItemAsync(item.Id);
            if (!(_dataStore is DbDataStore)) return;
            Peoples.Remove(item);
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        protected void OnCollectionChanged(NotifyCollectionChangedAction action, People item)
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, item));
        }
    }
}