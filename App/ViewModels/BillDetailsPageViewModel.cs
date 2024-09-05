using App.Models;
using App.Services;
using System.Windows.Input;

namespace App.ViewModels
{
    [QueryProperty(nameof(BillId), "billId")]
    public class BillDetailsPageViewModel : BindableObject
    {
        private readonly CosmosDbService _cosmosDbService;

        private string _billId;
        public string BillId
        {
            get => _billId;
            set
            {
                _billId = value;
                OnPropertyChanged();
            }
        }

        private BillViewModel _billViewModel;
        public BillViewModel BillViewModel
        {
            get => _billViewModel;
            set
            {
                _billViewModel = value;
                OnPropertyChanged();
            }
        }

        public ICommand ViewFullTextCommand { get; }

        public BillDetailsPageViewModel(CosmosDbService cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;
            ViewFullTextCommand = new Command(async () => await ViewFullText());
        }

        public async void LoadBill()
        {
            if (string.IsNullOrEmpty(BillId)) return;

            var bill = await _cosmosDbService.GetBillByIdAsync(BillId);
            if (bill != null)
            {
                BillViewModel = new BillViewModel(bill);
            }
            else
            {
                // Handle the case when the bill is not found
                await Application.Current.MainPage.DisplayAlert("Error", "Bill not found", "OK");
                await Shell.Current.GoToAsync("..");
            }
        }

        private async Task ViewFullText()
        {
            if (BillViewModel != null)
            {
                await Shell.Current.Navigation.PushAsync(new BillTextPage(this));
            }
        }
    }
}
