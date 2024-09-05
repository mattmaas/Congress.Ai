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
            GoBack();
            return true;
        }

        private void OnBackButtonClicked(object sender, EventArgs e)
        {
            GoBack();
        }

        private async void GoBack()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
