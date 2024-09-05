using System.Collections.ObjectModel;
using System.Windows.Input;
using CongressDataCollector.Core.Models;
using System.Linq;
using System;
using App.Services;

namespace App.ViewModels
{
    public class BillListPageViewModel : BindableObject
    {
        private readonly CosmosDbService _cosmosDbService;

        public ObservableCollection<BillViewModel> HouseBills { get; } = new ObservableCollection<BillViewModel>();
        public ObservableCollection<BillViewModel> SenateBills { get; } = new ObservableCollection<BillViewModel>();
        public ICommand GoToBillDetailsCommand { get; }

        public BillListPageViewModel(CosmosDbService cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;
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

        private async void LoadBills()
        {
            var bills = await _cosmosDbService.GetBillsAsync("SELECT * FROM c ORDER BY c.introducedDate DESC");

            foreach (var bill in bills)
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
