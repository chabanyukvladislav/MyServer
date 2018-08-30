using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace XamarinClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class AddUserPage : ContentPage
	{
		public AddUserPage ()
		{
			InitializeComponent ();
		    BindingContext = new AddUserViewModel();
		}
	}
}