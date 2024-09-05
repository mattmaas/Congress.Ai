using System.Collections.ObjectModel;
using System.Windows.Input;
using CongressDataCollector.Core.Models;
using System.Linq;
using System;

namespace App.ViewModels
{
    public class BillListPageViewModel : BindableObject
    {
        public ObservableCollection<BillViewModel> HouseBills { get; } = new ObservableCollection<BillViewModel>();
        public ObservableCollection<BillViewModel> SenateBills { get; } = new ObservableCollection<BillViewModel>();
        public ICommand GoToBillDetailsCommand { get; }

        public BillListPageViewModel()
        {
            GoToBillDetailsCommand = new Command<BillViewModel>(GoToBillDetails);
            LoadBills();
        }

        private async void GoToBillDetails(BillViewModel billViewModel)
        {
            if (billViewModel != null && !string.IsNullOrEmpty(billViewModel.Id))
            {
                await Shell.Current.GoToAsync($"BillDetailsPage?billId={billViewModel.Id}");
            }
        }

        private void LoadBills()
        {
            // TODO: Implement loading bills from your Cosmos DB
            // For now, we'll add some dummy data
            var bills = new List<Bill>
            {
                new Bill { Number = "H.R. 4250", Title = "PRESS Act", Type = "HR", IntroducedDate = DateTime.Parse("2023-06-21") },
                new Bill { Number = "S. 455", Title = "Travel Freedom Act", Type = "S", IntroducedDate = DateTime.Parse("2023-02-15") },
                // Add more dummy data as needed
            };

            foreach (var bill in bills.OrderByDescending(b => b.IntroducedDate))
            {
                var billViewModel = new BillViewModel(bill);
                if (bill.Type == "HR")
                {
                    HouseBills.Add(billViewModel);
                }
                else if (bill.Type == "S")
                {
                    SenateBills.Add(billViewModel);
                }
            }
        }
    }
}
