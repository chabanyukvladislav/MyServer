using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using Xamarin.Forms;
using XamarinClient.Models;
using XamarinClient.Services;

namespace XamarinClient.ViewModels
{
    class DialogViewModel : INotifyPropertyChanged
    {
        private readonly ServerDataStore _dataStore;
        private ObservableCollection<Message> _messages;
        private string _message;

        public string UserId { get; }
        public ObservableCollection<Message> Messages
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
            _dataStore = ServerDataStore.GetDataStore;
            UserId = userId;
            Message = "";
            Send = new Command(ExecuteSend);
            UpdateCollection();
        }

        private void UpdateCollection()
        {
            Messages = new ObservableCollection<Message>();
        }

        private void ExecuteSend()
        {
            if (string.IsNullOrWhiteSpace(Message))
                return;
            _dataStore.SendMessage(Message, UserId);
            Message = "";
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
