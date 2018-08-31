using System.Collections.ObjectModel;
using Xamarin.Forms;
using System.ComponentModel;
using System.Windows.Input;
using System.Collections.Specialized;
using System.Linq;
using XamarinClient.Collections;
using XamarinClient.Models;
using XamarinClient.Views;

namespace XamarinClient.ViewModels
{
    public class ItemsViewModel : INotifyPropertyChanged
    {
        private readonly PhonesCollection _collection;
        private readonly UsersCollection _usersCollection;
        private ObservableCollection<People> _items;
        private ObservableCollection<string> _userItems;
        private People _people;

        public ObservableCollection<People> Items
        {
            get => _items;
            private set
            {
                _items = value;
                OnPropertyChanged(nameof(Items));
            }
        }
        public ObservableCollection<string> UserItems
        {
            get => _userItems;
            private set
            {
                _userItems = value;
                OnPropertyChanged(nameof(UserItems));
            }
        }
        public People ItemSelected
        {
            get => _people;
            set
            {
                _people = value;
                OnPropertyChanged(nameof(ItemSelected));
            }
        }

        public ICommand AddItem { get; }
        public ICommand RemoveItem { get; }
        public ICommand UpdateItem { get; }
        public ICommand AddUserItem { get; }
        public ICommand Dialog { get; }

        public ItemsViewModel()
        {
            _collection = PhonesCollection.GetPhonesCollection;
            _usersCollection = UsersCollection.GetUsersCollection;
            AddItem = new Command(ExecuteAddItem);
            RemoveItem = new Command(ExecuteRemoveItem);
            UpdateItem = new Command(ExecuteUpdateItem);
            AddUserItem = new Command(ExecuteAddUserItem);
            Dialog = new Command(ExecuteDialog);
            _collection.CollectionChanged += ExecuteItemsRefresh;
            _usersCollection.CollectionChanged += ExecuteUsersRefresh;
        }

        private void ExecuteItemsRefresh(object sender, NotifyCollectionChangedEventArgs e)
        {
            Items = new ObservableCollection<People>(_collection.GetCollection());
        }

        private void ExecuteUsersRefresh(object sender, NotifyCollectionChangedEventArgs e)
        {
            UserItems = new ObservableCollection<string>(_usersCollection.GetCollection().Select(el => el.Nick));
        }


        private async void ExecuteAddItem()
        {
            ItemSelected = null;
            await Application.Current.MainPage.Navigation.PushAsync(new NewItemPage());
        }
        private async void ExecuteRemoveItem()
        {
            if (ItemSelected == null)
                return;
            if (!await Application.Current.MainPage.DisplayAlert("Delete?", "Are you sure?", "Yes", "No"))
                return;
            _collection.RemovePhone(ItemSelected);
            ItemSelected = null;
        }
        private async void ExecuteUpdateItem()
        {
            if (ItemSelected == null)
                return;
            People item = ItemSelected;
            ItemSelected = null;
            await Application.Current.MainPage.Navigation.PushAsync(new NewItemPage(item));
        }
        private async void ExecuteAddUserItem()
        {
            ItemSelected = null;
            await Application.Current.MainPage.Navigation.PushAsync(new AddUserPage());
        }
        private async void ExecuteDialog(object obj)
        {
            string userId = obj.ToString();
            await Application.Current.MainPage.Navigation.PushAsync(new DialogPage(userId));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}