using CongressDataCollector.Core.Models;

namespace App.ViewModels
{
    [QueryProperty(nameof(BillId), "billId")]
    public class BillDetailsPageViewModel : BindableObject
    {
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

        private void LoadBill(string billId)
        {
            // TODO: Implement loading bill details from your data source
            // For now, we'll use dummy data
            var bill = new Bill
            {
                Number = billId,
                Title = "Sample Bill " + billId,
                Type = billId.StartsWith("H") ? "H.R." : "S.",
                LatestAction = new LatestAction { Text = "Introduced in House" }
            };
            BillViewModel = new BillViewModel(bill);
        }
    }
}
