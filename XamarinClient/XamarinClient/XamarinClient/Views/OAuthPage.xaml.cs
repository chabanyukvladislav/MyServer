using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XamarinClient.ViewModels;

namespace XamarinClient.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class OAuthPage : ContentPage
	{
		public OAuthPage ()
		{
			InitializeComponent ();
		    Appearing += Focus;
		}

	    private void Focus(object sender, EventArgs e)
	    {
	        BindingContext = new OAuthViewModel();
	    }
	}
}