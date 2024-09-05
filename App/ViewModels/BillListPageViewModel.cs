using System.Collections.ObjectModel;
using System.Windows.Input;
using App.Models;
using System;
using System.Linq;
using App.Services;

namespace App.ViewModels
{
    public class BillListPageViewModel : BindableObject
    {
        private readonly CosmosDbService _cosmosDbService;
        private const int MaxBillsPerType = 50; // Limit to 50 bills per type

        private ObservableCollection<BillViewModel> _currentBills;
        public ObservableCollection<BillViewModel> CurrentBills
        {
            get => _currentBills;
            set
            {
                _currentBills = value;
                OnPropertyChanged();
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsNotLoading));
            }
        }

        public bool IsNotLoading => !IsLoading;

        public ObservableCollection<BillViewModel> HouseBills { get; } = new ObservableCollection<BillViewModel>();
        public ObservableCollection<BillViewModel> SenateBills { get; } = new ObservableCollection<BillViewModel>();
        public ICommand GoToBillDetailsCommand { get; }
        public ICommand ShowHouseBillsCommand { get; }
        public ICommand ShowSenateBillsCommand { get; }

        public BillListPageViewModel(CosmosDbService cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;
            GoToBillDetailsCommand = new Command<BillViewModel>(GoToBillDetails);
            ShowHouseBillsCommand = new Command(() => CurrentBills = HouseBills);
            ShowSenateBillsCommand = new Command(() => CurrentBills = SenateBills);
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
            IsLoading = true;

            var bills = await _cosmosDbService.GetBillsAsync($"SELECT TOP {MaxBillsPerType * 2} * FROM c ORDER BY c.introducedDate DESC");

            HouseBills.Clear();
            SenateBills.Clear();

            foreach (var bill in bills)
            {
                var fullBill = await _cosmosDbService.GetBillByIdAsync(bill.Id);
                var billViewModel = new BillViewModel(fullBill);
                if (bill.Type == "HR" && HouseBills.Count < MaxBillsPerType)
                {
                    HouseBills.Add(billViewModel);
                }
                else if (bill.Type == "S" && SenateBills.Count < MaxBillsPerType)
                {
                    SenateBills.Add(billViewModel);
                }

                if (HouseBills.Count >= MaxBillsPerType && SenateBills.Count >= MaxBillsPerType)
                {
                    break;
                }
            }

            CurrentBills = HouseBills; // Default to showing House bills
            IsLoading = false;
        }
    }
}
