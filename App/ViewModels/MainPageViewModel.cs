using System.Windows.Input;

namespace App.ViewModels
{
    public class MainPageViewModel
    {
        public ICommand GoToBillsCommand { get; }

        public MainPageViewModel()
        {
            GoToBillsCommand = new Command(GoToBills);
        }

        private async void GoToBills()
        {
            await Shell.Current.GoToAsync("//BillListPage");
        }
    }
}
