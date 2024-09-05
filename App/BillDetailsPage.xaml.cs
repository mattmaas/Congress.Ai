using App.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace App
{
    public partial class BillDetailsPage : TabbedPage
    {
        public BillDetailsPage(string billId)
        {
            InitializeComponent();
            var viewModel = App.Services.GetRequiredService<BillDetailsPageViewModel>();
            viewModel.BillId = billId;
            BindingContext = viewModel;
            Children.Add(new BillTextPage(viewModel));
        }

        protected override bool OnBackButtonPressed()
        {
            return base.OnBackButtonPressed();
        }

        private void OnBackButtonClicked(object sender, EventArgs e)
        {
            Navigation.PopAsync();
        }
    }
}
