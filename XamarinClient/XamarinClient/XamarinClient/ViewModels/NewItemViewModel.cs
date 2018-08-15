using System;
using System.Windows.Input;
using Xamarin.Forms;
using XamarinClient.Collections;
using XamarinClient.Models;
using static System.String;

namespace XamarinClient.ViewModels
{
    public class NewItemViewModel
    {
        private readonly PhonesCollection _collection;

        public People Item { get; set; }
        public ICommand Save { get; }

        public NewItemViewModel()
        {
            _collection = PhonesCollection.GetPhonesCollection;
            Save = new Command(ExecuteSave);
            Item = new People();
        }
        public NewItemViewModel(People item)
        {
            _collection = PhonesCollection.GetPhonesCollection;
            Save = new Command(ExecuteSave);
            Item = item;
        }

        private async void ExecuteSave()
        {
            if (IsNullOrWhiteSpace(Item.Name) || IsNullOrWhiteSpace(Item.Phone))
                return;
            if (Item.Id == Guid.Empty)
                _collection.AddPhone(Item);
            else
            {
                _collection.EditPhone(Item);
            }
            await Application.Current.MainPage.Navigation.PopAsync();
        }
    }
}