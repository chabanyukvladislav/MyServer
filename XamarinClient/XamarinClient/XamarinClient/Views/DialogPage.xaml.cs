using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinClient.ViewModels;

namespace XamarinClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DialogPage : ContentPage
	{
		public DialogPage (string userId)
		{
			InitializeComponent ();
		    BindingContext = new DialogViewModel(userId);
		}
	}
}