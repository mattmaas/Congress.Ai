using App.ViewModels;

namespace App
{
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();
            BindingContext = new SettingsViewModel();
        }

        protected override bool OnBackButtonPressed()
        {
            _ = GoBack();
            return true;
        }

        private async Task<bool> GoBack()
        {
            await Shell.Current.GoToAsync("//BillListPage");
            return true;
        }

        private async void OnBackClicked(object sender, EventArgs e)
        {
            await GoBack();
        }
    }
}
