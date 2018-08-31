using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using XamarinClient.Models;
using XamarinClient.Services;

namespace XamarinClient.ViewModels
{
    internal class UsersCollection : INotifyCollectionChanged
    {
        private const string HubAddress = "http://185.247.21.82:9090/Notification/";
        private static readonly object Locker = new object();
        private static UsersCollection _usersCollection;
        private readonly ServerDataStore _dataStore;
        private HubConnection _hubConnection;

        public static UsersCollection GetUsersCollection
        {
            get
            {
                if (_usersCollection == null)
                {
                    lock (Locker)
                    {
                        if (_usersCollection == null)
                        {
                            _usersCollection = new UsersCollection();
                        }
                    }
                }

                return _usersCollection;
            }
        }
        private List<PeopleUser> Users { get; set; }

        private UsersCollection()
        {
            _dataStore = ServerDataStore.GetDataStore;
            Users = new List<PeopleUser>();
            _hubConnection = new HubConnectionBuilder().WithUrl(HubAddress).Build();
            StartHub();
            UpdateCollection();
        }

        private async void UpdateCollection()
        {
            Users = await _dataStore.GetPeopleUsers();
            OnCollectionChanged(NotifyCollectionChangedAction.Reset, null);
        }

        private async void StartHub()
        {
            try
            {
                await _hubConnection.StartAsync();
                _hubConnection.On<string>("AddUser", (value) =>
                {
                    if (Users.Any(v => v.UserId == value)) return;
                    PeopleUser val = GetUser(value);
                    Users.Add(val);
                    OnCollectionChanged(NotifyCollectionChangedAction.Add, val);
                });
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private PeopleUser GetUser(string value)
        {
            return _dataStore.GetUser(value).Result;
        }

        public void Add(KeyValuePair<string, string> user)
        {
            _dataStore.AddPeopleUser(user);
        }

        public List<PeopleUser> GetCollection()
        {
            return Users;
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        protected void OnCollectionChanged(NotifyCollectionChangedAction action, PeopleUser item)
        {
            CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(action, item));
        }
    }
}