using App.ViewModels;

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
