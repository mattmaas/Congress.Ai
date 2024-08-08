using System.Collections.ObjectModel;
using System.Windows.Input;
using CongressDataCollector.Core.Models;

namespace App.ViewModels
{
    public class BillListPageViewModel
    {
        public ObservableCollection<Bill> Bills { get; } = new ObservableCollection<Bill>();
        public ICommand GoToBillDetailsCommand { get; }

        public BillListPageViewModel()
        {
            GoToBillDetailsCommand = new Command<Bill>(GoToBillDetails);
            // TODO: Load bills from your data source
            LoadBills();
        }

        private async void GoToBillDetails(Bill bill)
        {
            await Shell.Current.GoToAsync($"//BillDetailsPage?billId={bill.BillId}");
        }

        private void LoadBills()
        {
            // TODO: Implement loading bills from your data source
            // For now, we'll add some dummy data
            Bills.Add(new Bill { BillId = "1", BillNumber = "H.R. 1", Title = "Sample Bill 1" });
            Bills.Add(new Bill { BillId = "2", BillNumber = "S. 1", Title = "Sample Bill 2" });
        }
    }
}
