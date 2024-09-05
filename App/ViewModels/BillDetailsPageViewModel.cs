using App.Models;
using App.Services;
using System.Threading.Tasks;

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
                LoadBill(value);
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

        public BillDetailsPageViewModel(CosmosDbService cosmosDbService)
        {
            _cosmosDbService = cosmosDbService;
        }

        private async void LoadBill(string billId)
        {
            var bill = await _cosmosDbService.GetBillByIdAsync(billId);
            if (bill != null)
            {
                BillViewModel = new BillViewModel(bill);
            }
            else
            {
                // Handle the case when the bill is not found
                // You might want to show an error message or navigate back
            }
        }
    }
}
