using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinClient.Models;
using XamarinClient.ViewModels;

namespace XamarinClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NewItemPage : ContentPage
    {
        public NewItemPage()
        {
            InitializeComponent();
            NewItemViewModel vm = new NewItemViewModel();
            BindingContext = vm;
        }

        public NewItemPage(People item)
        {
            InitializeComponent();
            NewItemViewModel vm = new NewItemViewModel(item);
            BindingContext = vm;
        }
    }
}