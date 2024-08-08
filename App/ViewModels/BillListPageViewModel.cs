using System.Collections.ObjectModel;
using System.Windows.Input;
using CongressDataCollector.Core.Models;

namespace App.ViewModels
{
    public class BillListPageViewModel : BindableObject
    {
        public ObservableCollection<BillViewModel> Bills { get; } = new ObservableCollection<BillViewModel>();
        public ICommand GoToBillDetailsCommand { get; }

        public BillListPageViewModel()
        {
            GoToBillDetailsCommand = new Command<BillViewModel>(GoToBillDetails);
            LoadBills();
        }

        private async void GoToBillDetails(BillViewModel billViewModel)
        {
            await Shell.Current.GoToAsync($"//BillDetailsPage?billId={billViewModel.Id}");
        }

        private void LoadBills()
        {
            // TODO: Implement loading bills from your data source
            // For now, we'll add some dummy data
            Bills.Add(new BillViewModel(new Bill { Number = "H.R. 1", Title = "Sample Bill 1", Type = "H.R." }));
            Bills.Add(new BillViewModel(new Bill { Number = "S. 1", Title = "Sample Bill 2", Type = "S." }));
        }
    }
}
