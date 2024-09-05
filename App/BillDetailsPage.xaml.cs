using App.ViewModels;

namespace App
{
    public partial class BillDetailsPage : ContentPage
    {
        public BillDetailsPage()
        {
            InitializeComponent();
            BindingContext = App.Services.GetRequiredService<BillDetailsPageViewModel>();
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
