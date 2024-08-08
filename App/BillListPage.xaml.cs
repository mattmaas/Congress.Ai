using App.ViewModels;

namespace App
{
    public partial class BillListPage : ContentPage
    {
        public BillListPage()
        {
            InitializeComponent();
            BindingContext = new BillListPageViewModel();
        }

        public BillListPage(BillListPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}
