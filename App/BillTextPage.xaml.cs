using App.ViewModels;

namespace App
{
    public partial class BillTextPage : ContentPage
    {
        public BillTextPage(BillDetailsPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
