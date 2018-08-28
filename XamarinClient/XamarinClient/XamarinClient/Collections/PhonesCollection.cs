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
        private const string HubAddress = "http://185.247.21.82:9090/Notification/";
        private static readonly object Locker = new object();
        private static PhonesCollection _phonesCollection;
        private HubConnection _hubConnection;
        private IDataStore _dataStore;

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
            if (ServerDataStore.GetDataStore.IsConnect)
                ServerConnected();
            else
                DatabaseConnected();
        }

        private async void ServerConnected()
        {
            await Task.Run(() =>
            {
                try
                {
                    _dataStore.OnDisconnect -= ServerConnected;
                }
                catch (Exception)
                {
                    // ignored
                }

                _dataStore = ServerDataStore.GetDataStore;
                _dataStore.OnDisconnect += DatabaseConnected;
                while (_phonesCollection == null)
                {
                    Thread.Sleep(10);
                }
                _hubConnection = new HubConnectionBuilder().WithUrl(HubAddress).Build();
                StartHub();
                _dataStore.Synchronized();
                _dataStore.TryConnect();
                UpdateCollection();
            });
        }

        private async void DatabaseConnected()
        {
            await Task.Run(() =>
            {
                try
                {
                    _dataStore.OnDisconnect -= DatabaseConnected;
                }
                catch (Exception)
                {
                    // ignored
                }

                _dataStore = DbDataStore.GetDataStore;
                _dataStore.OnDisconnect += ServerConnected;
                while (_phonesCollection == null)
                {
                    Thread.Sleep(10);
                }
                _dataStore.Synchronized();
                _dataStore.TryConnect();
                UpdateCollection();
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
                // ignored
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
            DbDataStore dbds = (DbDataStore)_dataStore;
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