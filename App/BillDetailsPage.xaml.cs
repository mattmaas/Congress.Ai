using App.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace App
{
    public partial class BillDetailsPage : ContentPage
    {
        public BillDetailsPage(string billId)
        {
            InitializeComponent();
            var viewModel = App.Services.GetRequiredService<BillDetailsPageViewModel>();
            viewModel.BillId = billId;
            BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            (BindingContext as BillDetailsPageViewModel)?.LoadBill();
        }
    }
}
