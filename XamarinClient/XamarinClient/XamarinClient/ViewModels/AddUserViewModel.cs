using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Xamarin.Forms;
using XamarinClient.Services;
using XamarinClient.ViewModels;

namespace XamarinClient.Views
{
    internal class AddUserViewModel: INotifyPropertyChanged
    {
        private ServerDataStore _dataStore;
        private UsersCollection _collection;
        private ObservableCollection<string> _items;

        public ObservableCollection<string> Items
        {
            get => _items;
            set
            {
                _items = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Items)));
            }
        }

        public string ItemSelected
        {
            set
            {
                OnSelectedItemSet(value);
            }
        }

        private async void OnSelectedItemSet(string value)
        {
            _collection.Add(value);
            await Application.Current.MainPage.Navigation.PopAsync();
        }

        public AddUserViewModel()
        {
            _dataStore = ServerDataStore.GetDataStore;
            _collection = UsersCollection.GetUsersCollection;
            UpdateCollection();
        }

        private async void UpdateCollection()
        {
            List<string> data = await _dataStore.GetUsers();
            Items = new ObservableCollection<string>(data);
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
    }
}