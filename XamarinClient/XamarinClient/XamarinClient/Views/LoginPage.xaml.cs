using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinClient.ViewModels;

namespace XamarinClient.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class LoginPage : ContentPage
	{
		public LoginPage (string path)
		{
			InitializeComponent ();
            
		    BindingContext = new LoginViewModel(path);
		}
	}
}