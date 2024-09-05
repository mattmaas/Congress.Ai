using App.ViewModels;

namespace App
{
    public partial class BillTextPage : ContentPage
    {
        public BillTextPage()
        {
            InitializeComponent();
        }

        public BillTextPage(BillDetailsPageViewModel viewModel) : this()
        {
            BindingContext = viewModel;
        }
    }
}
