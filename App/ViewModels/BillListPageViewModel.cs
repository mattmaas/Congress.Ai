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
        private const int PageSize = 20;
        private int _currentPage = 0;
        private bool _isHouseBills = true;

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

        private bool _isFullViewMode = true;
        public bool IsFullViewMode
        {
            get => _isFullViewMode;
            set
            {
                _isFullViewMode = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TitleMaxLines));
            }
        }

        public int TitleMaxLines => IsFullViewMode ? 3 : 2;

        public ICommand GoToBillDetailsCommand { get; }
        public ICommand ShowHouseBillsCommand { get; }
        public ICommand ShowSenateBillsCommand { get; }
        public ICommand LoadMoreCommand { get; }
        public ICommand GoToSettingsCommand { get; }

        public BillListPageViewModel(CosmosDbService cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;
            CurrentBills = new ObservableCollection<BillViewModel>();
            GoToBillDetailsCommand = new Command<BillViewModel>(GoToBillDetails);
            ShowHouseBillsCommand = new Command(() => SwitchBillType(true));
            ShowSenateBillsCommand = new Command(() => SwitchBillType(false));
            LoadMoreCommand = new Command(async () => await LoadMoreBills());
            GoToSettingsCommand = new Command(GoToSettings);
            LoadInitialBills();

            SettingsViewModel.ViewModeChanged += OnViewModeChanged;
            IsFullViewMode = Preferences.Get("IsFullViewMode", true);
        }

        private void OnViewModeChanged(object sender, bool isFullViewMode)
        {
            IsFullViewMode = isFullViewMode;
        }

        private async void GoToBillDetails(BillViewModel billViewModel)
        {
            if (billViewModel != null && !string.IsNullOrEmpty(billViewModel.Id))
            {
                await Shell.Current.Navigation.PushAsync(new BillDetailsPage(billViewModel.Id));
            }
        }

        private async void GoToSettings()
        {
            await Shell.Current.Navigation.PushAsync(new SettingsPage());
        }

        private void SwitchBillType(bool isHouseBills)
        {
            if (_isHouseBills != isHouseBills)
            {
                _isHouseBills = isHouseBills;
                _currentPage = 0;
                CurrentBills = new ObservableCollection<BillViewModel>();
                LoadInitialBills();
            }
        }

        private async void LoadInitialBills()
        {
            IsLoading = true;
            CurrentBills.Clear();
            await LoadMoreBills();
            IsLoading = false;
        }

        private async Task LoadMoreBills()
        {
            if (IsLoading) return;

            IsLoading = true;
            var billType = _isHouseBills ? "HR" : "S";
            var bills = await _cosmosDbService.GetBillsAsync(billType, PageSize, _currentPage * PageSize);

            foreach (var bill in bills)
            {
                var fullBill = await _cosmosDbService.GetBillByIdAsync(bill.Id);
                var billViewModel = new BillViewModel(fullBill);
                CurrentBills.Add(billViewModel);
            }

            _currentPage++;
            IsLoading = false;
        }

        public void RefreshBills()
        {
            _currentPage = 0;
            CurrentBills.Clear();
            LoadInitialBills();
        }
    }
}
