using App.ViewModels;

namespace App
{
    public partial class BillListPage : ContentPage
    {
        public BillListPage(BillListPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
