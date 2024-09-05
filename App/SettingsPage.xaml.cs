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
            return GoBack();
        }

        private bool GoBack()
        {
            Shell.Current.GoToAsync("..").ConfigureAwait(false);
            return true;
        }

        private void OnBackClicked(object sender, EventArgs e)
        {
            GoBack();
        }
    }
}
