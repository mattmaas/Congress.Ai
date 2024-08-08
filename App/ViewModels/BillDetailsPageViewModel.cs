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

        private Bill _bill;
        public Bill Bill
        {
            get => _bill;
            set
            {
                _bill = value;
                OnPropertyChanged();
            }
        }

        private void LoadBill(string billId)
        {
            // TODO: Implement loading bill details from your data source
            // For now, we'll use dummy data
            Bill = new Bill
            {
                BillId = billId,
                BillNumber = "H.R. " + billId,
                Title = "Sample Bill " + billId,
                LatestAction = new LatestAction { Text = "Introduced in House" }
            };
        }
    }
}
