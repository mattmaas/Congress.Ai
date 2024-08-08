using App.ViewModels;

namespace App;

public partial class BillDetailsPage : ContentPage
{
	public BillDetailsPage(BillDetailsPageViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}
