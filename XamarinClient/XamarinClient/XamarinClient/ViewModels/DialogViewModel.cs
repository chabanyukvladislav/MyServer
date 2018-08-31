using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;
using XamarinClient.Models;
using XamarinClient.Services;

namespace XamarinClient.ViewModels
{
    class DialogViewModel : INotifyPropertyChanged
    {
        //private const string HubAddress = "http://localhost:6881/Notification/";
        private const string HubAddress = "http://185.247.21.82:9090/Notification/";
        private readonly ServerDataStore _dataStore;
        private readonly UsersCollection _collection;
        private ObservableCollection<Messager> _messages;
        private HubConnection _hubConnection;
        private string _message;

        public string UserNick { get; }
        public ObservableCollection<Messager> Messages
        {
            get => _messages;
            set
            {
                _messages = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Messages)));
            }
        }
        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Message)));
            }
        }

        public ICommand Send { get; set; }

        public DialogViewModel(string userId)
        {
            _collection = UsersCollection.GetUsersCollection;
            _dataStore = ServerDataStore.GetDataStore;
            UserNick = userId;
            Message = "";
            Send = new Command(ExecuteSend);
            UpdateCollection();
            _hubConnection = new HubConnectionBuilder().WithUrl(HubAddress).Build();
            StartHub();
        }

        private async void StartHub()
        {
            try
            {
                await _hubConnection.StartAsync();
                _hubConnection.On<Guid>("AddMessage", (value) =>
                {
                    if (Messages.Any(mes => mes.Id == value)) return;
                    Messager val = GetMessage(value);
                    Messages.Add(val);
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Messages)));
                });
            }
            catch (Exception)
            {
                // ignored
            }
        }

        private Messager GetMessage(Guid value)
        {
            return _dataStore.GetMessage(value).Result;
        }

        private async void UpdateCollection()
        {
            List<Messager> list = await _dataStore.GetMessages();
            PeopleUser toId = _collection.GetCollection().FirstOrDefault(el => el.Nick == UserNick);
            List<Messager> fromTo = list.Where(el => el.FromNick == UserNick || el.ToId == toId.Id).OrderBy(el=>el.Date).ToList();
            Messages = new ObservableCollection<Messager>(fromTo);
        }

        private void ExecuteSend()
        {
            if (string.IsNullOrWhiteSpace(Message))
                return;
            _dataStore.SendMessage(Message, _collection.GetCollection().FirstOrDefault(el => el.Nick == UserNick).Id);
            Message = "";
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
